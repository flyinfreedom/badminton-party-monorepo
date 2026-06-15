import { Injectable } from '@angular/core';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { ExecutionResultDialogComponent } from '../widgets/dialogs/execution-result-dialog/execution-result-dialog.component';
import { ExecutionDialogType } from '../enums';
import { ConfirmDialogComponent } from '../widgets/dialogs/confirm-dialog/confirm-dialog.component';
import { LoadingDialogComponent } from '../widgets/dialogs/loading-dialog/loading-dialog.component';

@Injectable({
  providedIn: 'root'
})
export class DialogService {

  constructor(private dialog: MatDialog) { }

  openSuccessDialog(title: string , message: string = ''): MatDialogRef<ExecutionResultDialogComponent> {
    return this.dialog.open(ExecutionResultDialogComponent, {
      width: '312px',
      data: {
        title: title,
        message: message,
        type: ExecutionDialogType.Successful,
      },
    });
  }

  openFailureDialog(title: string , message: string = ''): MatDialogRef<ExecutionResultDialogComponent> {
    return this.dialog.open(ExecutionResultDialogComponent, {
      width: '312px',
      data: {
        title: title,
        message: message,
        type: ExecutionDialogType.Failed,
      },
    });
  }

  openConfirmDialog(title: string, message: string = ''): MatDialogRef<ConfirmDialogComponent> {
    return this.dialog.open(ConfirmDialogComponent, {
      width: '100%',
      disableClose: true,
      data: {
        title: title,
        message: message
      },
    });
  }

  openLoadingDialog(): MatDialogRef<LoadingDialogComponent> {
    return this.dialog.open(LoadingDialogComponent, {
      disableClose: true
    });
  }
}
