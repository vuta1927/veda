import { NgModule } from '@angular/core';
import { SharedModule } from '../shared/shared.module';
import { RouterModule } from '@angular/router';
import { routes } from './project.routing';
import { ProjectComponent } from './project.component';
import { DxDataGridModule, DxMenuModule } from 'devextreme-angular';
import { CreateUpdateProjectComponent } from './create-update/create-update-project.component';
import { BlockUIModule } from 'ng-block-ui';
@NgModule({
    imports: [
        SharedModule, 
        RouterModule.forChild(routes), 
        DxDataGridModule, 
        DxMenuModule,
        BlockUIModule.forRoot()
        
    ],
    declarations: [
        ProjectComponent,
        CreateUpdateProjectComponent],
    entryComponents : [CreateUpdateProjectComponent]
})

export class ProjectModule {}