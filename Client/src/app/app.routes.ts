import { Routes } from '@angular/router';
import { PageNotFoundComponent } from './shared/components/page-not-found/page-not-found.component';
import { AuthGuard, ProjectGuard } from './shared/guards/auth.guard';
import { ThemeComponent } from './shared/components/theme.component';

export const ROUTES: Routes = [
  {
    path: '',
    component: ThemeComponent,
    data: { pageTitle: 'Home' },
    canActivate: [AuthGuard],
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      {
        path: 'dashboard',
        loadChildren: 'app/+dashboard/dashboard.module#DashboardModule',
        data: { pageTitle: 'Dashboard' }
      },
      {
        path: 'administrator',
        loadChildren: 'app/+administrator/administrator.module#AdministratorModule',
        data: { pageTitle: 'Administrator' }
      },
      {
        path: 'project',
        loadChildren: 'app/+project/project.module#ProjectModule'
      },
      {
        path: 'project-details',
        loadChildren: 'app/+project/project-details/project-details.module#ProjectDetailsModule'
      },
      {
        path: 'project-tag',
        loadChildren: 'app/+project/project-tag/project-tag.module#ProjectTagModule'
      },
      {
        path: 'project-info',
        loadChildren: 'app/+project/project-details/project-info/project-info.module#ProjectInfoModule'
      },
      {
        path: 'project-export',
        loadChildren: 'app/+project/project-details/project-import-export/export/export-project.module#ExportProjectModule'
      },
      {
        path: 'project-import',
        loadChildren: 'app/+project/project-details/project-import-export/import/import-project.module#ImportProjectModule'
      },
      {
        path: 'project-class',
        loadChildren: 'app/+project/project-details/project-class/project-class.module#ProjectClassModule'
      },
      {
        path: 'project-users',
        loadChildren: 'app/+project/project-details/project-users/project-users.module#ProjectUsersModule'
      },
      {
        path: 'merge-project',
        loadChildren: 'app/+project/merge-project/merge-project.module#MergeProjectModule',
      },
      {
        path: '404',
        loadChildren: 'app/shared/components/pages/default/not-found/not-found.module#NotFoundModule'
      }
    ]
  },

  {
    path: 'account',
    loadChildren: 'app/+account/account.module#AccountModule'
  },

  { 
    path: '**',
    redirectTo: '404',
    pathMatch: 'full'
  }
];
