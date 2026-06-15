import { ExecutionDialogType } from './../../../enums/execution-dialog-type';
import { Component, Inject, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { IExecutionResult } from '../../../models/dialogs/execution-result-model';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-execution-result-dialog',
  templateUrl: './execution-result-dialog.component.html',
  styleUrls: ['./execution-result-dialog.component.scss'],
  standalone: true,
  imports: [CommonModule, MatDialogModule]
})
export class ExecutionResultDialogComponent implements OnInit {
  ExecutionDialogTypeEnum = ExecutionDialogType;

  constructor(
    public dialogRef: MatDialogRef<ExecutionResultDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: IExecutionResult) {

  }

  ngOnInit(): void {
      if(this.data.type === ExecutionDialogType.Successful) {
        setTimeout(() => {
          this.dialogRef.close();
        }, 1000);
      }
  }

  close(): void {
    this.dialogRef.close();
  }
}
