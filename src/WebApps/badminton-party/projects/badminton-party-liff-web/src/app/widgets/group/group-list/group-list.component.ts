import { Component, Input, OnInit } from '@angular/core';
import { IGroup } from '../../../models';
import { CommonModule } from '@angular/common';
import { GroupCardComponent } from '../group-card/group-card.component';

interface IMyGroupByDate {
  date: string;
  groups: Array<IGroup>;
}

@Component({
  selector: 'app-group-list',
  templateUrl: './group-list.component.html',
  styleUrls: ['./group-list.component.scss'],
  standalone: true,
  imports: [CommonModule, GroupCardComponent]
})
export class GroupListComponent implements OnInit {
  @Input('groups')
  groups: Array<IGroup> = [];

  @Input('show-avatar')
  showAvatar: boolean = true;

  @Input('is-sort-asc')
  isSortAsc: boolean = false;

  groupByDate: Array<IMyGroupByDate> = [];

  ngOnInit(): void {
    this.groups.sort((a, b) => a.startTime.localeCompare(b.startTime)).forEach(group => {
      const key = group.startTime.split('T')[0];
      let myGroupByDate = this.groupByDate.find(g => g.date === key);
      if (!myGroupByDate) {
        myGroupByDate = { date: key, groups: [] };
        this.groupByDate.push(myGroupByDate);
      }
      myGroupByDate.groups.push(group);
    });

    this.groupByDate.sort((a, b) => this.isSortAsc ? b.date.localeCompare(a.date) :  a.date.localeCompare(b.date));
  }
}
