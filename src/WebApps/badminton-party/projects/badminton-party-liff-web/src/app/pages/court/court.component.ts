import { Component, OnInit, inject } from '@angular/core';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { ICourt, IGroup } from '../../models';
import { ApiService } from '../../services/api.service';
import { LiffService } from '../../services/liff.service';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { GroupListComponent } from '../../widgets/group/group-list/group-list.component';

@Component({
  selector: 'app-court',
  templateUrl: './court.component.html',
  styleUrls: ['./court.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatIconModule,
    MatSnackBarModule,
    GroupListComponent
  ]
})
export class CourtComponent implements OnInit {
  courtId: string = '';
  court: ICourt | null = null;
  groups: IGroup[] = [];

  private apiService = inject(ApiService);
  private liffService = inject(LiffService);
  private snackBar = inject(MatSnackBar);
  private route = inject(ActivatedRoute);

  constructor() { }

  ngOnInit(): void {
    this.courtId = this.route.snapshot.params['courtId'];
    this.apiService.getCourtById(this.courtId).subscribe(res => {
      this.court = res;
    });

    this.apiService.getGroupsByCourtId(this.courtId).subscribe(res => {
      this.groups = res;
    })
  }

  copyUrl(): void {
    const currentUrl: string = window.location.href;
    const url: URL = new URL(currentUrl);
    const ssrUrl = `${this.liffService.getLiffUrl()}/ssr/court/${this.courtId}?t=${new Date().getTime()}`;

    var textarea = document.createElement("textarea");
    textarea.value = ssrUrl;
    document.body.appendChild(textarea);
    textarea.select();
    document.execCommand('copy');
    document.body.removeChild(textarea);
    this.snackBar.open('複製成功', 'close', {
      duration: 1500,
      panelClass: ['custom-snackbar']
    });
  }

  share() {
    const buttonsTemplate = {
      type: 'buttons',
      thumbnailImageUrl: this.court!.avatar,
      title: this.court!.courtName,
      text: `目前有 ${this.groups.length} 個開放的羽球團`,
      actions: [
        {
          type: 'uri',
          label: `查看${this.court!.courtName}`,
          uri: `${this.liffService.getLiffUrl()}/court/${this.courtId}`
        }
      ]
    };
    this.liffService.shareGroup(buttonsTemplate, this.court!.courtName);
  }
}
