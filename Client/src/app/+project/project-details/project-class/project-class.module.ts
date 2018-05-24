import { NgModule } from '@angular/core';
import { SharedModule } from '../../../shared/shared.module';
import { RouterModule } from '@angular/router';
import { routes } from './project-class.routing';
import { DxDataGridModule, DxMenuModule } from 'devextreme-angular';
import { ProjectClassComponent } from './project-class.component';
import { CreateUpdateClassComponent } from './create-update/create-update-class.component';
import { ColorPickerModule } from 'ngx-color-picker';
// import { DataService } from './data.service';
@NgModule({
    imports: [
        SharedModule,
        RouterModule.forChild(routes), 
        DxDataGridModule, 
        DxMenuModule,
        ColorPickerModule,
    ],
    declarations: [ProjectClassComponent, CreateUpdateClassComponent],
    entryComponents : [CreateUpdateClassComponent],
    providers: []
})

export class ProjectClassModule {}