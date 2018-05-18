import { ActivatedRoute, Routes } from '@angular/router';
import { DefaultComponent } from '../../shared/components/pages/default/default.component';
import { SettingsComponent } from './settings.component';
import { AuthGuard } from '../../shared/guards/auth.guard';

export const routes: Routes = [
    { 
        path: '',
        component: DefaultComponent,
        canActivate: [AuthGuard],
        children: [
            {
                path: '',
                component: SettingsComponent
            }
        ]
    }
];