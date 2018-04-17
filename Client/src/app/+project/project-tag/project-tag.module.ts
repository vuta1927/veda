import { NgModule } from '@angular/core';
import { SharedModule } from '../../shared/shared.module';
import { RouterModule } from '@angular/router';
import { routes } from './project-tag.routing';
import { ProjecTagComponent } from './project-tag.component';
// import { DataService } from './data.service';
@NgModule({
    imports: [SharedModule, RouterModule.forChild(routes)],
    declarations: [ProjecTagComponent,],
    entryComponents : [],
    providers: []
})

export class ProjectTagModule {}