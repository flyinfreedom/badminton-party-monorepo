import { DatePipe, CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { IGroup } from '../../models';
import { ApiService } from '../../services/api.service';
import { MatTabsModule } from '@angular/material/tabs';
import { MatIconModule } from '@angular/material/icon';
import { GroupListComponent } from '../../widgets/group/group-list/group-list.component';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-history',
  templateUrl: './history.component.html',
  styleUrls: ['./history.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatTabsModule,
    MatIconModule,
    GroupListComponent
  ],
  providers: [DatePipe]
})
export class HistoryComponent implements OnInit {
    createdGroup: Array<IGroup> = [];
    joinedGroups: Array<IGroup> = [];
    init: boolean = false;

    private apiService = inject(ApiService);
    private datePipe = inject(DatePipe);

    constructor() { }

    ngOnInit() {
      const now = new Date();
      this.apiService.getMyHistoryGroup(+this.datePipe.transform(now, 'yyyyMM')!).subscribe(res => {
        this.createdGroup = res.createdGroups;
        this.joinedGroups = res.joinedGroups;
        this.init = true;
      });
    }
}
