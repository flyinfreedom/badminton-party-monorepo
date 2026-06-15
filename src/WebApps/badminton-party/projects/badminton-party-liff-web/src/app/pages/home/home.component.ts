import { MatTabGroup, MatTabsModule } from '@angular/material/tabs';
import { MatIconModule } from '@angular/material/icon';
import { HubService } from './../../services/hub.service';
import { Component, OnDestroy, OnInit, ViewChild, inject } from '@angular/core';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { take } from 'rxjs';
import { IGroup, IGroupMember } from './../../models';
import { ApiService } from './../../services/api.service';
import { GroupListComponent } from './../../widgets/group/group-list/group-list.component';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterModule, MatTabsModule, MatIconModule, GroupListComponent],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent implements OnInit, OnDestroy {
  createdGroup: Array<IGroup> = [];
  joinedGroups: Array<IGroup> = [];
  inited: boolean = false;

  @ViewChild('tabGroup') tabGroup!: MatTabGroup;

  private readonly apiService = inject(ApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly hubService = inject(HubService);

  constructor() { }

  ngOnInit(): void {
    this.apiService.getMyCurrentGroup().subscribe(res => {
      this.createdGroup = res.createdGroups;
      this.joinedGroups = res.joinedGroups;
      this.inited = true;
      this.hubService.hubConnection!.invoke('JoinHomeGroup', [...res.createdGroups.map(g => g.groupId), ...res.joinedGroups.map(g => g.groupId)]);

      if (this.createdGroup.length !== 0) {
        this.route.url.pipe(take(1)).subscribe(url => {
          if (url[0]?.path === 'my-group') {
            this.tabGroup.selectedIndex = 1;
          }
        });
      }
    });

    this.hubService.hubConnection!.on('home_group', message => {
      let group = this.createdGroup.find(g => g.groupId === message.groupId);
      if (!group) {
        group = this.joinedGroups.find(g => g.groupId === message.groupId);
      }

      if (!!group) {
        group.joinedMembers.splice(0, group.joinedMembers.length);
        for (let i = 0; i < message.count; i++) {
          group.joinedMembers.push({} as IGroupMember);
        }
      }
    });
  }

  ngOnDestroy(): void {
    this.hubService.hubConnection!.invoke('LeaveHomeGroup', [...this.createdGroup.map(g => g.groupId), ...this.joinedGroups.map(g => g.groupId)])
  }
}
