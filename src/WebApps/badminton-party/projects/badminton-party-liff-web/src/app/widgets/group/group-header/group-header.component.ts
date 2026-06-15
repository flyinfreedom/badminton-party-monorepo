import { Component, Input } from '@angular/core';
import { IGroup } from '../../../models';
import { CommonModule } from '@angular/common';
import { ProgressBarComponent } from '../progress-bar/progress-bar.component';

@Component({
  selector: 'app-group-header',
  templateUrl: './group-header.component.html',
  styleUrls: ['./group-header.component.scss'],
  standalone: true,
  imports: [CommonModule, ProgressBarComponent]
})
export class GroupHeaderComponent {
  @Input()
  group?: IGroup;

  constructor() {}

  getCurrentJoinPeople(): number {
    return Math.min(this.group!.joinedMembers.length, this.group!.maxPeople);
  }
}
