
import { RouterModule, Routes } from '@angular/router';
import { DefaultComponent } from '../shared/components/pages/default/default.component';
import { AnalyticsComponent } from './analytics/analytics.component';
import { AuthGuard } from '../shared/guards/auth.guard';

export const routes: Routes = [
    { 
        path: '',
        component: DefaultComponent,
        canActivate: [AuthGuard],
        children: [
            {
                path: '',
                component: AnalyticsComponent
            }
        ]
    }
];