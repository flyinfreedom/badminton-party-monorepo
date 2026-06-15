import { Component, ElementRef, OnInit, ViewChild, inject } from '@angular/core';
import { ProfileService } from '../../services/profile.service';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ApiService } from '../../services/api.service';
import { DialogService } from '../../services/dialog.service';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-user-setting',
  templateUrl: './user-setting.component.html',
  styleUrls: ['./user-setting.component.scss'],
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatIconModule]
})
export class UserSettingComponent implements OnInit {
  @ViewChild('fileInput') fileInput!: ElementRef;

  memberForm!: FormGroup;

  isNameEdit: boolean = false;

  public profileService = inject(ProfileService);
  private dialogService = inject(DialogService);
  private apiService = inject(ApiService);
  private fb = inject(FormBuilder);

  constructor() {
    this.memberForm = this.fb.group({
      memberName: ['', Validators.required]
    });
  }
  ngOnInit(): void {
      this.memberForm.controls['memberName'].setValue(this.profileService.profile?.displayName);
  }

  openFileInput() {
    this.fileInput.nativeElement.click();
  }

  onFileSelected(event: any) {
    const dialogRef = this.dialogService.openLoadingDialog();
    const file: File = event.target.files[0];
    const formData = new FormData();
    formData.append('file', file, file.name);

    this.apiService.uploadAvatar(formData).subscribe(res => {
      dialogRef.close();
      this.profileService.pictureUrl = res.avatarUrl;
      this.dialogService.openSuccessDialog('上傳成功');
    },
    error => {
      dialogRef.close();
      this.fileInput.nativeElement.value = null;
      this.dialogService.openFailureDialog('圖片上傳失敗');
    })
  }

  toggleNameInput(): void {
    this.isNameEdit = !this.isNameEdit;
  }

  updateUserName(): void {
    this.apiService.updateUserName(this.memberForm.controls['memberName'].value).subscribe(res => {
      this.profileService.profile!.displayName = res;
      this.dialogService.openSuccessDialog('更新完成');
    });
  }
}
