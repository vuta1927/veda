import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { SharedModule } from '../shared/shared.module';
import { AnalyticsComponent } from './analytics/analytics.component';
import { AuthGuard } from '../shared/guards/auth.guard';
import { DefaultComponent } from '../shared/components/pages/default/default.component';

const routes: Routes = [
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

@NgModule({
    declarations: [
        AnalyticsComponent
    ],
    imports: [
        SharedModule,
        RouterModule.forChild(routes)
    ]
})
export class DashboardModule {}
