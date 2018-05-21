import { NgModule } from '@angular/core';
import { SharedModule } from '../../shared/shared.module';
import { RouterModule } from '@angular/router';
import { routes } from './project-details.routing';
import { ProjectDetailsComponent } from './project-details.component';
import { DxDataGridModule, DxMenuModule } from 'devextreme-angular';
import { ProjectInfoComponent } from './project-info/project-info.component';
import { ProjectImagesComponent } from './project-images/project-images.component';
import { ProjectUsersComponent } from './project-users/project-users.component';
import { ProjectClassComponent } from './project-class/project-class.component';
import { ImportProjectComponent } from './project-import-export/import/import-project.component';
import { ExportProjectComponent } from './project-import-export/export/export-project.component';
import { DataService } from './data.service';
import { CreateUpdateProjectUserComponent } from './project-users/create-update-project-user/create-update-project-user.component';
import { CreateUpdateClassComponent } from './project-class/create-update/create-update-class.component';
import { ColorPickerModule } from 'ngx-color-picker';
import { CustomQcStatus } from './project-images/custom-pipe.pipe';
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
        ProjectInfoComponent, 
        ProjectImagesComponent, 
        ProjectUsersComponent, 
        ProjectClassComponent, 
        CreateUpdateProjectUserComponent, 
        CreateUpdateClassComponent, 
        ImportProjectComponent, 
        ExportProjectComponent,
        CustomQcStatus
    ],
    entryComponents : [CreateUpdateProjectUserComponent, CreateUpdateClassComponent],
    providers: [DataService]
})

export class ProjectDetailsModule {}