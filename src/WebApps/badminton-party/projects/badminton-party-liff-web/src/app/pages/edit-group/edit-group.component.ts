import { AfterViewInit, Component, ElementRef, OnDestroy, OnInit, ViewChild, inject } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { GroupStatus } from '../../enums';
import { IGroup } from '../../models';
import { ApiService } from '../../services/api.service';
import { DialogService } from '../../services/dialog.service';
import { HubService } from '../../services/hub.service';
import { ProfileService } from '../../services/profile.service';
import { GroupFormComponent } from '../../widgets/forms/group-form/group-form.component';
import { CommonModule } from '@angular/common';
import { MemberListComponent } from '../../widgets/group/member-list/member-list.component';

@Component({
  selector: 'app-edit-group',
  templateUrl: './edit-group.component.html',
  styleUrls: ['./edit-group.component.scss'],
  standalone: true,
  imports: [CommonModule, RouterModule, GroupFormComponent, MemberListComponent]
})
export class EditGroupComponent implements OnInit, OnDestroy, AfterViewInit {
  valid: boolean = false;
  groupId: string = '';
  group?: IGroup;

  @ViewChild('divider')
  dividerElementRef!: ElementRef;

  @ViewChild('groupForm')
  groupForm!: GroupFormComponent;

  editorOffsetTop: number = 0;
  memberListOffsetTop: number = 0;
  editable: boolean = true;

  isOverTime: boolean = false;
  timer?: NodeJS.Timeout;

  GroupStatus = GroupStatus;

  private dialogService = inject(DialogService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private apiService = inject(ApiService);
  private hubService = inject(HubService);
  private profileService = inject(ProfileService);

  constructor() {
  }

  ngOnInit(): void {
    this.groupId = this.route.snapshot.params['groupId'];
    this.apiService.getGroupById(this.groupId).subscribe(res => {
      if (res.memberId !== this.profileService.profile?.memberId) {
        this.dialogService.openFailureDialog('您沒有權限');
        this.router.navigate(['/']);
        return;
      }

      if (res.groupStatus === GroupStatus.Closed) {
        this.router.navigate([`/group/${res.groupId}/view`]);
      }

      this.group = res;

      this.checkIsOverTime();
      this.handleEditable();
      this.timer = setInterval(() => {
        this.checkIsOverTime();
        this.handleEditable();
      }, 1000);

      if (!this.isOverTime) {
        this.hubService.hubConnection!.invoke('JoinGroup', this.groupId);
        this.hubService.hubConnection!.on('group', message => {
          this.group!.joinedMembers.splice(0, this.group!.joinedMembers.length);
          this.group!.joinedMembers = message;
        });
      }
    });
  }

  handleEditable(): void {
    this.editable = !this.isOverTime;
  }

  checkIsOverTime(): void {
    this.isOverTime = (new Date().getTime() - new Date(this.group!.endTime).getTime()) >= 0
  }

  ngAfterViewInit(): void {
    this.memberListOffsetTop = this.dividerElementRef.nativeElement.offsetTop;
  }

  checkValidation(valid: boolean) {
    this.valid = valid;
  }

  scrollToTarget(targetPosition: number): void {
    window.scrollTo({
      top: targetPosition - 127, //-- 扣除menu 和 sub-nav 的高度
      behavior: 'smooth'
    });
  }

  submit(): void {
    if (!this.valid) {
      return;
    }

    const result = this.groupForm.getFormResult();
    if (!result) {
      return;
    }

    this.apiService.updateGroup(this.groupId, result).subscribe(requestContent => {
      this.dialogService.openSuccessDialog('更新成功');
      this.profileService.handleRecentOpening(requestContent.courtId, requestContent.courtName, requestContent.location);
      this.router.navigate([`/group/${this.groupId}/view`]);
    });
  }

  removed(memberId: string): void { }

  closeGroup(): void {
    this.apiService.closeGroup(this.groupId).subscribe(res => {
      if (res) {
        this.dialogService.openSuccessDialog('關閉成功');
        this.router.navigate(['/']);
      }
    });
  }

  ngOnDestroy(): void {
    clearInterval(this.timer!);
    this.hubService.hubConnection!.invoke('LeaveGroup', this.groupId);
  }
}
