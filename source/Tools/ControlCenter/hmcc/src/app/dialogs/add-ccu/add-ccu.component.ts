import { Component } from '@angular/core';
import {FormBuilder, FormControl, FormGroup, Validators} from "@angular/forms";
import {CcuRepositoryService} from "../../services/ccu-repository.service";
import {DialogRef} from "@angular/cdk/dialog";

@Component({
  selector: 'hmcc-add-ccu',
  templateUrl: './add-ccu.component.html',
  styleUrls: ['./add-ccu.component.scss']
})
export class AddCcuComponent {
  ccuFormGroup: FormGroup = this.formBuilder.group<AddCcuData>({
    name: this.formBuilder.control('', [Validators.required]),
    url: this.formBuilder.control('', [Validators.required, Validators.pattern('/^((https?:)(\\/\\/\\/?)([\\w]*(?::[\\w]*)?@)?([\\d\\w\\.-]+)(?::(\\d+))?)?([\\/\\\\\\w\\.()-]*)?(?:([?][^#]*)?(#.*)?)*/gmi')])
    }
  );

  constructor(private formBuilder: FormBuilder, private ccuRepository: CcuRepositoryService,
              private dialogRef: DialogRef) {
  }

  addCcu() {
    if (this.ccuFormGroup.invalid) {
      return;
    }

    const ccu = this.ccuFormGroup.value;

    this.ccuRepository.addCcu(ccu).subscribe(() => {
      this.dialogRef.close();
    });
  }
}

interface AddCcuData {
  name: FormControl<string | null>,
  url: FormControl<string | null>
}
