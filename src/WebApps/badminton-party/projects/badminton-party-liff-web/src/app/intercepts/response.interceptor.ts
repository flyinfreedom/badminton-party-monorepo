import {
  HttpErrorResponse,
  HttpEvent,
  HttpHandler,
  HttpInterceptor,
  HttpRequest,
  HttpResponse,
} from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, empty, finalize, throwError,  } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { ExecutionResultDialogComponent } from '../widgets/dialogs/execution-result-dialog/execution-result-dialog.component';
import { LoadingDialogComponent } from '../widgets/dialogs/loading-dialog/loading-dialog.component';
import { ExecutionDialogType } from '../enums';

@Injectable()
export class ResponseInterceptor implements HttpInterceptor {
  private dialogRef!: MatDialogRef<LoadingDialogComponent>;
  private errorDialogRef: MatDialogRef<ExecutionResultDialogComponent> | null = null;
  constructor(public dialog: MatDialog) { }

  intercept(
    req: HttpRequest<any>,
    next: HttpHandler
  ): Observable<HttpEvent<any>> {
    if (!(req.url.trim() === 'api/login' || req.url.trim().startsWith('api/identity'))) {
      this.openDialog();
    }
    return next.handle(req).pipe(
      catchError((error: HttpErrorResponse) => {
        if(error.status === 700) {
          this.openErrorDialog(error.error.message);
        }
        throw error;
      }),
      finalize(() => {
        this.closeLoadingDialog();
      })
    );
  }


  openDialog(): void {
    if (!!this.dialogRef && this.dialogRef.getState() == 0) {
      return;
    }
    this.dialogRef = this.dialog.open(LoadingDialogComponent, {
      disableClose: true
    });
  }

  closeLoadingDialog(): void {
    if(!this.dialogRef) {
      return;
    }
    this.dialogRef.close();
  }

  openErrorDialog(message: string): void {
    if (!!this.errorDialogRef && this.errorDialogRef.getState() == 0) {
      return;
    }
    this.errorDialogRef = this.dialog.open(ExecutionResultDialogComponent, {
      disableClose: false,
      data: {
        title: '',
        message: message,
        type: ExecutionDialogType.Failed
      }
    });
  }
}
