import { IGroup } from '../../../models';
import { Component, Input, OnInit } from '@angular/core';
import { GroupStatus } from '../../../enums';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { ProgressBarComponent } from '../progress-bar/progress-bar.component';
import { TimeRangePipe } from '../../../pipes/time-range.pipe';
import { LevelPipe } from '../../../pipes/level.pipe';
import { ConsumptionPatternsPipe } from '../../../pipes/consumption-patterns.pipe';

@Component({
  selector: 'app-group-card',
  templateUrl: './group-card.component.html',
  styleUrls: ['./group-card.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatIconModule,
    ProgressBarComponent,
    TimeRangePipe,
    LevelPipe,
    ConsumptionPatternsPipe
  ]
})
export class GroupCardComponent implements OnInit {
  @Input()
  showAvatar: boolean = true;

  @Input()
  group!: IGroup;

  GroupStatus = GroupStatus;

  constructor() {

  }

  ngOnInit(): void { }
}
