import {Component} from '@angular/core';
import {CcuRepositoryService} from "../services/ccu-repository.service";
import {CcuModel} from "../services/ccu-model";
import {firstValueFrom} from "rxjs";

@Component({
  selector: 'hmcc-ccu-list-overview',
  templateUrl: './ccu-list-overview.component.html',
  styleUrls: ['./ccu-list-overview.component.scss']
})
export class CcuListOverviewComponent {
  ccuModels: CcuModel[] = [];

  constructor(private ccuRepository: CcuRepositoryService) {
    //this.ccuInfos.push({name: 'CCU2', ipAddress: '192.168.2.210'});
  }

  openCcuEditDialog(ccu: CcuModel) {

  }

  deleteCcu(ccu: CcuModel) {
    this.ccuRepository.removeCcu(ccu).subscribe(() => {
      this.updateCcuModels();
    });
  }

  async openCcuNewDialog() {
    await this.ccuRepository.addCcu({name: 'CCU3', url: '', id: undefined});

    this.updateCcuModels();
  }

  updateCcuModels(){
    this.ccuRepository.getAllCcu().subscribe(ccuModels => {
      this.ccuModels = ccuModels;
    });
  }
}
