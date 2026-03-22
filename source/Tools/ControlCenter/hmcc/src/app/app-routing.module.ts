import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import {DashboardComponent} from "./dashboard/dashboard.component";
import {CcuListOverviewComponent} from "./ccu-list-overview/ccu-list-overview.component";

const routes: Routes = [
  { path: '', component: DashboardComponent },
  { path: 'ccu-list-overview', component: CcuListOverviewComponent}
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
