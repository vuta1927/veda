import { NgModule } from '@angular/core';
import { SharedModule } from '../../shared/shared.module';
import { RouterModule } from '@angular/router';
import { routes } from './merge-project.routing';
import { DxDataGridModule, DxMenuModule } from 'devextreme-angular';
import { MergeProject } from './merge-project.component';
// import { DataService } from './data.service';
@NgModule({
    imports: [SharedModule, RouterModule.forChild(routes), DxDataGridModule, DxMenuModule],
    declarations: [MergeProject],
    entryComponents : [],
    providers: []
})

export class MergeProjectModule {}