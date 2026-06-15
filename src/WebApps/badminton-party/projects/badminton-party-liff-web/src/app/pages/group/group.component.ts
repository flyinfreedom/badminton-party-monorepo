import { Component, ElementRef, OnDestroy, OnInit, ViewChild, inject } from '@angular/core';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { ApiService } from '../../services/api.service';
import { LiffService } from '../../services/liff.service';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { IGroup } from '../../models';
import { ProfileService } from '../../services/profile.service';
import { CommonModule, DatePipe } from '@angular/common';
import { DialogService } from '../../services/dialog.service';
import { ConsumptionPatterns, GroupStatus } from '../../enums';
import { HubService } from '../../services/hub.service';
import { MatIconModule } from '@angular/material/icon';
import { GroupHeaderComponent } from '../../widgets/group/group-header/group-header.component';
import { MemberListComponent } from '../../widgets/group/member-list/member-list.component';
import { TimeRangePipe } from '../../pipes/time-range.pipe';
import { ConsumptionPatternsPipe } from '../../pipes/consumption-patterns.pipe';
import { LevelPipe } from '../../pipes/level.pipe';

@Component({
  selector: 'app-group',
  templateUrl: './group.component.html',
  styleUrls: ['./group.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatIconModule,
    MatSnackBarModule,
    GroupHeaderComponent,
    MemberListComponent,
    TimeRangePipe,
    ConsumptionPatternsPipe,
    LevelPipe
  ],
  providers: [DatePipe]
})
export class GroupComponent implements OnInit, OnDestroy {
  isOwner: boolean = true;
  group: IGroup | null = null;
  groupId: string = '';
  isOverTime: boolean = false;

  timer?: NodeJS.Timeout;

  @ViewChild('divider')
  dividerElementRef!: ElementRef;

  private route = inject(ActivatedRoute);
  private snackBar = inject(MatSnackBar);
  private liffService = inject(LiffService);
  private hubService = inject(HubService);
  private apiService = inject(ApiService);
  private profileService = inject(ProfileService);
  private dialogService = inject(DialogService);
  private router = inject(Router);
  public datePipe = inject(DatePipe);

  constructor() { }

  ngOnInit(): void {
    this.groupId = this.route.snapshot.params['groupId'];
    this.apiService.getGroupById(this.groupId).subscribe(res => {
      this.group = res;
      this.isOwner = this.group.memberId === this.profileService.profile!.memberId;
      const action = this.route.snapshot.params['action'];

      this.checkIsOverTime();
      this.timer = setInterval(() => {
        this.checkIsOverTime();
      }, 1000);

      if (this.isOverTime || this.group.groupStatus === GroupStatus.Closed) {
        return;
      }

      if (action === 'join') {
        if (this.group.joinedMembers.filter(member => member.memberId === this.profileService.profile?.memberId).length !== 0) {
          this.dialogService.openConfirmDialog('您已經加入過囉，是否要再加 1人?').afterClosed().subscribe(confirmResult => {
            confirmResult && this.join();
          });
          return;
        }

        this.join();
        this.router.navigate([`/group/${this.groupId}/view`]);
      }

      if (action === 'first' && this.group.joinedMembers.length === 0) {
        this.dialogService.openConfirmDialog('您是否也加入一起打球?').afterClosed().subscribe(confirmResult => {
          confirmResult && this.join()
        });
      }

      if (!this.isOverTime) {
        this.hubService.hubConnection!.invoke('JoinGroup', this.groupId);

        this.hubService.hubConnection!.on('group', message => {
          this.group!.joinedMembers.splice(0, this.group!.joinedMembers.length);
          this.group!.joinedMembers = message;
        });
      }
    });
  }

  checkIsOverTime(): void {
    this.isOverTime = (new Date().getTime() - new Date(this.group!.endTime).getTime()) >= 0
  }

  hasJoined(): boolean {
    if (!this.group) {
      return false;
    }
    return this.group!.joinedMembers.findIndex(member => member.memberId == this.profileService.profile!.memberId) !== -1;
  }

  copyUrl(): void {
    const currentUrl: string = window.location.href;
    const url: URL = new URL(currentUrl);
    const ssrUrl = `${this.liffService.getLiffUrl()}/ssr/group/${this.groupId}?t=${new Date().getTime()}`;

    var textarea = document.createElement("textarea");
    textarea.value = ssrUrl;
    document.body.appendChild(textarea);
    textarea.select();
    document.execCommand('copy');
    document.body.removeChild(textarea);
    this.snackBar.open('複製成功', 'close', {
      duration: 1500,
      panelClass: ['custom-snackbar']
    });
  }

  join(): void {
    this.apiService.joinGroup(this.groupId).subscribe(res => {
      const myMembers = res.filter(member => member.memberId === this.profileService.profile?.memberId);
      this.dialogService.openSuccessDialog(myMembers.length === 1 ? '加入成功' : '+1 成功');
      this.group!.joinedMembers = res;
    });
  }

  minusOneMember(): void {
    this.apiService.minusOneMember(this.groupId).subscribe(res => {
      this.dialogService.openSuccessDialog('-1 成功');
      this.group!.joinedMembers = res;
    });
  }

  leaveAll(): void {
    const myMember = this.group?.joinedMembers.filter(member => member.memberId === this.profileService.profile?.memberId);
    if (myMember?.length == 1) {
      this.callLeaveAll();
      return;
    }

    this.dialogService.openConfirmDialog('確定將所有人退出嗎?')
      .afterClosed()
      .subscribe(res => {
        res === true && this.callLeaveAll();
      });
  }

  callLeaveAll(): void {
    this.apiService.leaveGroup(this.groupId).subscribe(res => {
      this.group!.joinedMembers = res;
      this.dialogService.openSuccessDialog('退出成功');
    })
  }

  leaveButtonWording(): string {
    var joinCount = this.group!.joinedMembers.filter(m => m.memberId === this.profileService.profile?.memberId).length;
    return joinCount == 1 ? '離開' : '-1人';
  }

  joinButtonWording(): string {
    if(this.group!.joinedMembers.length < this.group!.maxPeople) {
      return '+1 人';
    }

    if(this.group!.joinedMembers.length >= this.group!.maxPeople && this.group!.joinedMembers.length < this.group!.maxPeople + this.group!.alternatePeople) {
      return '加入候補'
    }

    return '已額滿';
  }

  isOpeningGroup(): boolean {
    return this.group!.groupStatus === GroupStatus.Opened;
  }

  share() {
    const joinUrl = `${this.liffService.getLiffUrl()}/group/${this.groupId}/join`;
    const viewUrl = `${this.liffService.getLiffUrl()}/group/${this.groupId}/view`;
    let payment = '';
    switch (this.group!.consumptionPatterns) {
      case ConsumptionPatterns.Fixed:
        payment = this.group!.amount.toString();
        break;
      case ConsumptionPatterns.AA:
        payment = '平分';
        break;
      case ConsumptionPatterns.Free:
        payment = '免費';
        break;
    }

    const buttonsTemplate = {
      type: 'buttons',
      thumbnailImageUrl: this.group!.avatar,
      title: this.group!.groupName,
      text: `時間：${this.datePipe.transform(this.group!.startTime, 'yyyy/MM/dd HH')}:00~${this.datePipe.transform(this.group!.endTime, 'HH')}:00 \n球館：${this.group!.courtName}\n費用：${payment} `,
      actions: [
        {
          type: 'uri',
          label: '加入',
          uri: joinUrl
        },
        {
          type: 'uri',
          label: '查看內容',
          uri: viewUrl
        }
      ]
    };
    this.liffService.shareGroup(buttonsTemplate, this.group!.groupName);
  }

  scrollToTarget(): void {
    window.scrollTo({
      top: this.dividerElementRef.nativeElement.offsetTop - 67, //-- 扣除menu
      behavior: 'smooth'
    });
  }

  ngOnDestroy(): void {
    clearInterval(this.timer!);
    this.hubService.hubConnection!.invoke('LeaveGroup', this.groupId);
  }
}
