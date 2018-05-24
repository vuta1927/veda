import { NgModule } from '@angular/core';
import { SharedModule } from '../../shared/shared.module';
import { RouterModule } from '@angular/router';
import { routes } from './project-details.routing';
import { ProjectDetailsComponent } from './project-details.component';
import { DxDataGridModule, DxMenuModule } from 'devextreme-angular';
import { ProjectImagesComponent } from './project-images/project-images.component';
import { ColorPickerModule } from 'ngx-color-picker';
@NgModule({
    imports: [
        SharedModule, 
        RouterModule.forChild(routes), 
        DxDataGridModule, 
        DxMenuModule, 
        ColorPickerModule,
    ],
    declarations: [
        ProjectDetailsComponent, 
        ProjectImagesComponent, 
    ],
    entryComponents : [],
    providers: []
})

export class ProjectDetailsModule {}