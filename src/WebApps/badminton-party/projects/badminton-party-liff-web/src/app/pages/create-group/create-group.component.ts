import { AfterViewInit, Component, Renderer2, ViewChild, inject } from '@angular/core';
import { Router } from '@angular/router';
import { ApiService } from '../../services/api.service';
import { DialogService } from '../../services/dialog.service';
import { ProfileService } from '../../services/profile.service';
import { GroupFormComponent } from '../../widgets/forms/group-form/group-form.component';

@Component({
  selector: 'app-create-group',
  templateUrl: './create-group.component.html',
  styleUrls: ['./create-group.component.scss'],
  standalone: true,
  imports: [GroupFormComponent]
})
export class CreateGroupComponent implements AfterViewInit {
  @ViewChild('groupForm')
  groupForm!: GroupFormComponent;

  valid: boolean = false;

  private renderer = inject(Renderer2);
  private router = inject(Router);
  private dialogService = inject(DialogService);
  private profileService = inject(ProfileService);
  private apiService = inject(ApiService);

  constructor() {
  }

  ngAfterViewInit(): void {
    this.scrollToTop();
  }

  checkValidation(valid: boolean) {
    this.valid = valid;
  }

  submit(): void {
    const requestContent = this.groupForm.getFormResult();
    if (!requestContent) {
      return;
    }

    this.apiService.createGroup(requestContent).subscribe(groupId => {
      const dialogRef = this.dialogService.openSuccessDialog('開團成功');
      this.profileService.handleRecentOpening(requestContent.courtId, requestContent.courtName, requestContent.location);
      dialogRef.afterClosed().subscribe(_ => this.router.navigate([`/group/${groupId}/first`]))
    });
  }

  private scrollToTop() {
    this.renderer.setProperty(document.documentElement, 'scrollTop', 0);
    this.renderer.setProperty(document.body, 'scrollTop', 0);
  }
}
