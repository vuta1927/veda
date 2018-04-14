import { Routes } from '@angular/router';
import { AuthGuard, ProjectGuard } from '../shared/guards/auth.guard';
import { DefaultComponent } from '../shared/components/pages/default/default.component';
import { ProjectComponent } from './project.component';
export const routes: Routes = [
    {
        path: '',
        component: DefaultComponent,
        children: [
            {
                path: '',
                component: ProjectComponent
            },
            {
                path: '/',
                component: ProjectComponent
            }
        ]
    }
]