import { NgModule } from '@angular/core';
import { SharedModule } from '../../../shared/shared.module';
import { RouterModule } from '@angular/router';
import { routes } from './project-info.routing';
import { DxDataGridModule, DxMenuModule } from 'devextreme-angular';
import { ProjectInfoComponent } from './project-info.component';
// import { DataService } from './data.service';
@NgModule({
    imports: [SharedModule, RouterModule.forChild(routes), DxDataGridModule, DxMenuModule],
    declarations: [ProjectInfoComponent,],
    entryComponents : [],
    providers: []
})

export class ProjectInfoModule {}