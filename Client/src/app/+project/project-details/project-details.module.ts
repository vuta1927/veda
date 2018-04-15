import { NgModule } from '@angular/core';
import { SharedModule } from '../../shared/shared.module';
import { RouterModule } from '@angular/router';
import { routes } from './project-details.routing';
import { ProjectDetailsComponent } from './project-details.component';
import { DxDataGridModule, DxMenuModule } from 'devextreme-angular';
import { ProjectInfoComponent } from './project-info/project-info.component';
import { ProjectImagesComponent } from './project-images/project-images.component';
import { DataService } from './data.service';
@NgModule({
    imports: [SharedModule, RouterModule.forChild(routes), DxDataGridModule, DxMenuModule],
    declarations: [ProjectDetailsComponent, ProjectInfoComponent, ProjectImagesComponent],
    entryComponents : [],
    providers: [DataService]
})

export class ProjectDetailsModule {}