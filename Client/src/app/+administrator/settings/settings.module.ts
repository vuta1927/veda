import { NgModule } from '@angular/core';

import { SharedModule } from '../../shared/shared.module';
import { SettingsComponent } from './settings.component';
import { routes } from './settings.routing';
import { RouterModule, Routes } from '@angular/router';
@NgModule({
    declarations: [
        SettingsComponent
    ],
    imports: [
        SharedModule,
        RouterModule.forChild(routes)
    ]
})
export class SettingsModule {}
