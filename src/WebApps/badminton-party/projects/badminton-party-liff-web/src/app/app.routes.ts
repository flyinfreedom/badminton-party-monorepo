import { Routes } from '@angular/router';
import { LayoutComponent } from './widgets/layout/layout.component';
import { HomeComponent } from './pages/home/home.component';
import { CreateGroupComponent } from './pages/create-group/create-group.component';
import { EditGroupComponent } from './pages/edit-group/edit-group.component';
import { HistoryComponent } from './pages/history/history.component';
import { GroupComponent } from './pages/group/group.component';
import { CourtComponent } from './pages/court/court.component';
import { UserSettingComponent } from './pages/user-setting/user-setting.component';
import { SearchCourtComponent } from './pages/search-court/search-court.component';

export const routes: Routes = [{
  path: '',
  component: LayoutComponent,
  children: [
    {
      path: '',
      component: HomeComponent
    },
    {
      path: 'my-group',
      component: HomeComponent
    },
    {
      path: 'search-court',
      component: SearchCourtComponent
    },
    {
      path: 'court/:courtId',
      component: CourtComponent
    },
    {
      path: 'create-group',
      component: CreateGroupComponent
    },
    {
      path: 'edit-group/:groupId',
      component: EditGroupComponent
    },
    {
      path: 'group/:groupId/:action',
      component: GroupComponent
    },
    {
      path: 'history',
      component: HistoryComponent
    },
    {
      path: 'user-setting',
      component: UserSettingComponent
    }
  ]
}];
