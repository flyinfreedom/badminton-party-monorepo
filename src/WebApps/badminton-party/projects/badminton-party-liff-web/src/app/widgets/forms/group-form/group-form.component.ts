import { DatePipe, CommonModule } from '@angular/common';
import { AfterViewChecked, Component, EventEmitter, Input, OnChanges, OnInit, Output, SimpleChanges, inject } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, ValidationErrors, ValidatorFn, Validators, ReactiveFormsModule } from '@angular/forms';
import { Observable, map, startWith } from 'rxjs';
import { ConsumptionPatterns, LevelGroup } from '../../../enums';
import { ICourt, IGroup, IGroupFormRequest, ISelectOption } from '../../../models';
import { ApiService } from '../../../services/api.service';
import { DialogService } from '../../../services/dialog.service';
import { ProfileService } from '../../../services/profile.service';
import { StringExtensions } from '../../../utils/string.extension';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatRadioModule } from '@angular/material/radio';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatInputModule } from '@angular/material/input';
import { LevelPipe } from '../../../pipes/level.pipe';

interface IAuthCompleteOption {
  label: string;
  options: Array<ICourt>;
}

@Component({
  selector: 'app-group-form',
  templateUrl: './group-form.component.html',
  styleUrls: ['./group-form.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatAutocompleteModule,
    MatRadioModule,
    MatSlideToggleModule,
    MatInputModule,
    LevelPipe
  ],
  providers: [DatePipe]
})
export class GroupFormComponent implements OnInit, OnChanges, AfterViewChecked {
  @Output('valid')
  validEmitted: EventEmitter<boolean> = new EventEmitter<boolean>();

  @Input()
  group?: IGroup;

  @Input()
  editable: boolean = true;

  groupForm!: FormGroup;
  startTimeOptions: Array<ISelectOption<number>> = [];
  hourOptions!: Array<number>;
  minPeopleOptions!: Array<number>;
  maxPeopleOptions!: Array<number>;
  levelGroupOptions!: Array<ISelectOption<LevelGroup>>;
  minDay: Date = new Date(new Date().setHours(new Date().getHours() + 1));
  now: Date = new Date();

  recentOpenings: ICourt[] = [];
  courtOptions: ICourt[] = [];

  filteredOptions!: Observable<IAuthCompleteOption[]>;

  private fb = inject(FormBuilder);
  private datePipe = inject(DatePipe);
  private apiService = inject(ApiService);
  private dialogService = inject(DialogService);
  private profileService = inject(ProfileService);

  constructor() {
    this.groupForm = this.fb.group({
      groupName: ['', [Validators.required, Validators.maxLength(12)]],
      date: [this.minDay, Validators.required],
      startTime: [(this.now.getHours() + 1) % 24, Validators.required],
      playTime: [2, Validators.required],
      consumptionPatterns: [2, Validators.required],
      amount: ['', [Validators.max(9999), Validators.pattern('^[0-9]*$')]],
      minPeople: [2, [Validators.required, Validators.min(2), Validators.max(999), Validators.pattern('^[0-9]*$')]],
      maxPeople: [8, [Validators.required, Validators.min(2), Validators.max(999), Validators.pattern('^[0-9]*$')]],
      alternatePeople: [0, [Validators.required, Validators.min(0), Validators.max(999), Validators.pattern('^[0-9]*$')]],
      courtId: [0],
      courtName: ['', [Validators.required, Validators.maxLength(12)]],
      location: ['', Validators.maxLength(100)],
      levelGroup: [LevelGroup.NotLimited, Validators.required],
      isPrivate: [false, Validators.required],
      otherInfo: ['', Validators.maxLength(300)]
    }, { validators: [amountRequiredValidator(), overPeopleValidator(), alternatePeopleValidator()] });

    this.generateTimeOption();
    this.hourOptions = Array.from({ length: 8 }, (_, index) => index + 1);
    this.minPeopleOptions = Array.from({ length: 6 }, (_, index) => (index + 1) * 2);
    this.maxPeopleOptions = Array.from({ length: 16 }, (_, index) => (index + 1) * 2);
    this.levelGroupOptions = this.generateLevelGroupOption();

    this.groupForm.valueChanges.subscribe(_ => {
      this.validEmitted.emit(this.groupForm.valid);
    })

    this.recentOpenings = this.profileService.profile!.recentOpenings.sort((a, b) => {
      return new Date(b.openingTime).getTime() - new Date(a.openingTime).getTime()
    }).map(ro => {
      return {
        courtId: ro.courtId,
        courtName: ro.courtName,
        location: ro.location
      } as ICourt;
    });
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (!this.group) {
      return;
    }

    this.initGroup(this.group);
    this.handleControls();
  }

  handleControls()
  {
    if (this.editable) {
      for(const fieldName in this.groupForm.controls) {
        this.groupForm.controls[fieldName].enable();
      }
    }
    else
    {
      for(const fieldName in this.groupForm.controls) {
        this.groupForm.controls[fieldName].disable();
      }
    }
  }

  ngAfterViewChecked(): void {
    const optionsMin = Math.min(...this.startTimeOptions.map(option => option.value));
    const startTimeControl = this.groupForm.get('startTime');
    if(optionsMin > +startTimeControl!.value) {
      startTimeControl!.setValue(optionsMin);
      return;
    }
    startTimeControl!.setValue(startTimeControl!.value);
  }

  ngOnInit(): void {
    this.apiService.getCourts().subscribe(res => {
      this.courtOptions = res;
      this.filteredOptions = this.groupForm.controls['courtName'].valueChanges.pipe(
        startWith(''),
        map(value => this._filter(value || '')),
      );
    })
  }

  public getFormResult(): IGroupFormRequest | null {
    if (this.groupForm.invalid) {
      this.groupForm.markAllAsTouched();
      this.dialogService.openFailureDialog('尚有必填欄位未填寫或驗證錯誤');
      return null;
    }

    const result = Object.assign({}, this.groupForm.value);

    const allCourt = [...this.courtOptions, this.recentOpenings] as ICourt[];
    result.courtId = `${this.profileService.profile?.memberId}_${new Date().getTime()}`;
    const court = allCourt.find(court => court.courtName === result.courtName && court.location === result.location);
    if (!!court) {
      result.courtId = court.courtId;
    }

    result.location = result.location ?? '';
    result.levelGroup = +result.levelGroup;
    result.startTime = `${this.datePipe.transform(result.date, 'yyyy-MM-dd')}T${StringExtensions.padLeft(result.startTime, 2)}:00:00`;
    if (result.consumptionPatterns !== ConsumptionPatterns.Fixed) {
      result.amount = 0;
    }
    delete result.date;
    return result as IGroupFormRequest;
  }

  private _filter(value: string): IAuthCompleteOption[] {
    const filterValue = value.toLowerCase();
    const recentOpenings = this.recentOpenings.filter(option => option.courtName.includes(filterValue));
    const result: IAuthCompleteOption[] = [];
    if (recentOpenings.length !== 0) {
      result.push(
        {
          label: '最近加入',
          options: recentOpenings
        });
    }
    result.push({
      label: !value ? '為您推薦' :'為您找到',
      options: this.courtOptions.filter(option => option.courtName.includes(filterValue) && recentOpenings.findIndex(ro => ro.courtId === option.courtId) === -1)
    });
    return result;
  }

  private initGroup(group: IGroup): void {
    this.groupForm.setValue({
      groupName: group.groupName,
      date: new Date(group.startTime.split('T')[0]),
      startTime: +group.startTime.split('T')[1].split(':')[0],
      playTime: group.playTime,
      consumptionPatterns: group.consumptionPatterns,
      amount: group.amount,
      minPeople: group.minPeople,
      maxPeople: group.maxPeople,
      alternatePeople: group.alternatePeople,
      courtId: group.courtId,
      courtName: group.courtName,
      location: group.location ?? '',
      levelGroup: group.levelGroup,
      isPrivate: group.isPrivate,
      otherInfo: group.otherInfo ?? ''
    });

    this.generateTimeOption();
  }

  selectCourt(value: any) {
    const allCourtOptions = [...this.courtOptions, ...this.recentOpenings];
    const court = allCourtOptions.find(c => c.courtId === value);
    this.groupForm.controls['courtId'].setValue(court!.courtId);
    this.groupForm.controls['courtName'].setValue(court!.courtName);
    this.groupForm.controls['location'].setValue(court!.location);
  }

  getTimeInterval(): number {
    return +this.groupForm.value.endTime > +this.groupForm.value.startTime
      ? +this.groupForm.value.endTime - +this.groupForm.value.startTime
      : (+this.groupForm.value.endTime + 24) - +this.groupForm.value.startTime;
  }

  generateTimeOption(): void {
    const now = new Date();
    const hourArray: Array<ISelectOption<number>> = [];
    const formStartDay = this.datePipe.transform(this.groupForm.value.date, 'yyyyMMdd');
    const today = this.datePipe.transform(now, 'yyyyMMdd');
    const isToday = formStartDay === today;
    const startHour = !!this.group ? new Date(this.group.startTime).getHours() : (this.now.getHours() + 1) % 24;

    this.startTimeOptions.splice(0, this.startTimeOptions.length);

    for (let h = 0; h < 24; h++) {
      if (isToday && h < startHour) {
        continue;
      }
      this.startTimeOptions.push({ value: h, label: `${StringExtensions.padLeft(h, 2)}:00` })
    }
  }

  private generateLevelGroupOption(): Array<ISelectOption<LevelGroup>> {
    const LevelGroupEnum = LevelGroup;
    const result: Array<ISelectOption<LevelGroup>> = [];

    for (const levelGroup in LevelGroupEnum) {
      if (!isNaN(+levelGroup)) {
        result.push({ value: +levelGroup, label: levelGroup })
      }
    }
    return result;
  }
}


function amountRequiredValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const consumptionPatternsControl = control.get('consumptionPatterns');
    const amountControl = control.get('amount');

    if (!!consumptionPatternsControl && consumptionPatternsControl.value === ConsumptionPatterns.Fixed) {
      return !!amountControl && !amountControl.value ? { 'requiredAmount': true } : null;
    }

    return null;
  };
}

function overPeopleValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const maxPeople = +control.get('maxPeople')!.value;
    const minPeople = +control.get('minPeople')!.value;
    return maxPeople < minPeople ? { 'overPeople': true } : null;
  };
}

function alternatePeopleValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const maxPeople = +control.get('maxPeople')!.value;
    const alternatePeople = +control.get('alternatePeople')!.value;
    return maxPeople < alternatePeople ? { 'alternatePeopleOver': true } : null;
  };
}
