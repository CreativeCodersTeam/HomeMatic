import {Component, OnInit} from '@angular/core';
import {CcuRepositoryService} from "../services/ccu-repository.service";
import {CcuModel} from "../services/ccu-model";
import {firstValueFrom} from "rxjs";
import {MatDialog} from "@angular/material/dialog";
import {AddCcuComponent} from "../dialogs/add-ccu/add-ccu.component";

@Component({
  selector: 'hmcc-ccu-list-overview',
  templateUrl: './ccu-list-overview.component.html',
  styleUrls: ['./ccu-list-overview.component.scss']
})
export class CcuListOverviewComponent implements OnInit{
  ccuModels: CcuModel[] = [];

  constructor(private ccuRepository: CcuRepositoryService, private dialog: MatDialog){
  }

  openCcuEditDialog(ccu: CcuModel) {

  }

  deleteCcu(ccu: CcuModel) {
    this.ccuRepository.removeCcu(ccu).subscribe(() => {
      this.updateCcuModels();
    });
  }

  openCcuNewDialog() {
    const ccu: CcuModel = {
      name: 'CCU3', url: 'http://192.168.2.210', id: undefined
    };

    this.dialog.open(AddCcuComponent);

    // this.ccuRepository.addCcu(ccu)
    //   .subscribe(() => {
    //     console.log(ccu.id);
    //     this.updateCcuModels();
    //   });
  }

  updateCcuModels(){
    this.ccuRepository.getAllCcu().subscribe(ccuModels => {
      this.ccuModels = ccuModels;
    });
  }

  ngOnInit(): void {
    this.updateCcuModels();
  }
}
