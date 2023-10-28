import { Component } from '@angular/core';
import {FormBuilder, FormGroup} from "@angular/forms";

@Component({
  selector: 'hmcc-add-ccu',
  templateUrl: './add-ccu.component.html',
  styleUrls: ['./add-ccu.component.scss']
})
export class AddCcuComponent {
  ccuFormGroup: FormGroup = this.formBuilder.group<AddCcuData>(
    {
      name: '',
      url: ''
    }
  );

  constructor(private formBuilder: FormBuilder) {
  }

}

interface AddCcuData {
  name: string,
  url: string
}
