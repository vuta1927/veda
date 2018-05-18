import { NgModule } from '@angular/core';

import { SharedModule } from '../shared/shared.module';
import { AnalyticsComponent } from './analytics/analytics.component';
import { routes } from './dashboard.routing';
import { RouterModule, Routes } from '@angular/router';
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
