import { Component } from '@angular/core';
import {CcuInfo} from "./ccu-info";

@Component({
  selector: 'hmcc-ccu-list-overview',
  templateUrl: './ccu-list-overview.component.html',
  styleUrls: ['./ccu-list-overview.component.scss']
})
export class CcuListOverviewComponent {
  ccuInfos: CcuInfo[] = [];

  constructor() {
    this.ccuInfos.push({name: 'CCU2', ipAddress: '192.168.2.210'});
  }

  openCcuEditDialog(ccu: CcuInfo) {

  }

  deleteCcu(ccu: CcuInfo) {

  }

  openCcuNewDialog() {
    this.ccuInfos.push({name: 'CCU3', ipAddress: '192.168.2.211'});
  }
}
