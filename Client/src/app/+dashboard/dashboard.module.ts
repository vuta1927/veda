import { NgModule } from '@angular/core';

import { SharedModule } from '../shared/shared.module';
import { AnalyticsComponent } from './analytics/analytics.component';
import { routes } from './dashboard.routing';
import { RouterModule, Routes } from '@angular/router';
import { DxDataGridModule } from 'devextreme-angular';
@NgModule({
    declarations: [
        AnalyticsComponent
    ],
    imports: [
        SharedModule,
        RouterModule.forChild(routes),
        DxDataGridModule,
    ]
})
export class DashboardModule {}
