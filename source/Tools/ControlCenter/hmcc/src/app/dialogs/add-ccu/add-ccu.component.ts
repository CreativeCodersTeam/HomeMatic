import { Component } from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  FormControl,
  FormGroup,
  ValidationErrors,
  Validator,
  Validators
} from "@angular/forms";
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
    url: this.formBuilder.control('', [Validators.required, CustomValidators.url])
    //url: this.formBuilder.control('', [Validators.required, Validators.pattern('/^((https?:)(\\/\\/\\/?)([\\w]*(?::[\\w]*)?@)?([\\d\\w\\.-]+)(?::(\\d+))?)?([\\/\\\\\\w\\.()-]*)?(?:([?][^#]*)?(#.*)?)*/gmi')])
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

    this.ccuRepository.addCcu(ccu).subscribe(
      {
        next: () => {
          this.dialogRef.close();
        },
        error: error => {
          console.error(error);
        }
      }
    );
  }
}

export class CustomValidators {
  static url(control: AbstractControl): ValidationErrors | null {
    try {
      // noinspection HttpUrlsUsage
      if (!control.value.startsWith('http://') && !control.value.startsWith('https://')) {
        return {invalidUrl: true};
      }
      new URL(control.value);
      return null;
    }
    catch {
      return {invalidUrl: true};
    }
  }
}

interface AddCcuData {
  name: FormControl<string | null>,
  url: FormControl<string | null>
}
