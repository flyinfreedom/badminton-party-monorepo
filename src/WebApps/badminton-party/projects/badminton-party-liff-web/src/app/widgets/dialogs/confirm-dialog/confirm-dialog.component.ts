import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { IConfirmInfo } from '../../../models';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-confirm-dialog',
  templateUrl: './confirm-dialog.component.html',
  styleUrls: ['./confirm-dialog.component.scss'],
  standalone: true,
  imports: [CommonModule, MatDialogModule]
})
export class ConfirmDialogComponent {
  confirmedText: string = '確定';
  rejectedText: string = '取消';

  constructor(
    public dialogRef: MatDialogRef<ConfirmDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: IConfirmInfo
  ) {}

  ngOnInit(): void {
    this.confirmedText = this.data.confirmedText ?? this.confirmedText;
    this.rejectedText = this.data.rejectedText ?? this.rejectedText;
  }

  confirmed(): void {
    this.dialogRef.close(true);
  }

  closeDialog(): void {
    this.dialogRef.close(false);
  }
}
