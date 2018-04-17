import { NgModule } from '@angular/core';
import { SharedModule } from '../../shared/shared.module';
import { RouterModule } from '@angular/router';
import { routes } from './project-details.routing';
import { ProjectDetailsComponent } from './project-details.component';
import { DxDataGridModule, DxMenuModule } from 'devextreme-angular';
import { ProjectInfoComponent } from './project-info/project-info.component';
import { ProjectImagesComponent } from './project-images/project-images.component';
import { ProjectUsersComponent } from './project-users/project-users.component';
import { DataService } from './data.service';
import { CreateUpdateProjectUserComponent } from './project-users/create-update-project-user/create-update-project-user.component';
@NgModule({
    imports: [SharedModule, RouterModule.forChild(routes), DxDataGridModule, DxMenuModule],
    declarations: [ProjectDetailsComponent, ProjectInfoComponent, ProjectImagesComponent, ProjectUsersComponent, CreateUpdateProjectUserComponent],
    entryComponents : [CreateUpdateProjectUserComponent],
    providers: [DataService]
})

export class ProjectDetailsModule {}