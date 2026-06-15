import { Component, EventEmitter, Input, OnInit, Output, inject } from '@angular/core';
import { IGroupMember } from '../../../models';
import { ApiService } from '../../../services/api.service';
import { DialogService } from '../../../services/dialog.service';
import { ProfileService } from '../../../services/profile.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-member-list',
  templateUrl: './member-list.component.html',
  styleUrls: ['./member-list.component.scss'],
  standalone: true,
  imports: [CommonModule]
})
export class MemberListComponent implements OnInit {
  @Input('edit-mode')
  editMode: boolean = false;

  @Input('groupId')
  groupId!: string;

  @Input("member-list")
  _memberList: Array<IGroupMember> = [];

  @Input('min-people')
  minPeople: number = 0;

  @Input('max-people')
  maxPeople: number = 0;

  public get memberList(): Array<IGroupMember> {
    return this._memberList.sort((a, b) => {
      return new Date(a.joinTime).getTime() - new Date(b.joinTime).getTime()
    });
  }

  @Output('remove')
  removeEmitter: EventEmitter<string> = new EventEmitter();

  private dialogService = inject(DialogService);
  private apiService = inject(ApiService);
  public profileService = inject(ProfileService);

  constructor() { }

  ngOnInit(): void {
    console.log(this.minPeople);
  }

  removeMember(memberId: string): void {
    const member = this._memberList.find(member => member.memberId === memberId);
    const dialogRef = this.dialogService.openConfirmDialog(`您確定要移除 ${member?.displayName} 嗎?`);
    dialogRef.afterClosed().subscribe(res => {
      if (!!res) {
        this.apiService.removeGroupMember(this.groupId, member!.memberId).subscribe(removeRes => {
          this._memberList = removeRes;
          this.removeEmitter.emit(memberId);
        });
      }
    });
  }
}
