import { Routes } from '@angular/router';
import { AuthGuard } from '../shared/guards/auth.guard';
import { DefaultComponent } from '../shared/components/pages/default/default.component';

export const routes: Routes = [
    {
        path: '',
        canActivate: [AuthGuard],
        // component: DefaultComponent,
        children: [
            {
                path: 'users',
                loadChildren: './users/users.module#UsersModule'
            },
            {
                path: 'roles',
                loadChildren: './roles/roles.module#RolesModule'
            },
            {
                path: 'settings',
                loadChildren: './settings/settings.module#SettingsModule'
            }
        ]
    }
]