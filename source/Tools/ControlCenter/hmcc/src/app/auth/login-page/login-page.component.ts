import { Component } from '@angular/core';
import {FormBuilder, FormControl} from "@angular/forms";
import {AuthService} from "./auth.service";
import {Router} from "@angular/router";

interface LoginData {
  userName: FormControl,
  password: FormControl
}

@Component({
  selector: 'hmcc-login-page',
  templateUrl: './login-page.component.html',
  styleUrls: ['./login-page.component.scss']
})
export class LoginPageComponent {
  loginFormGroup = this.fb.group<LoginData>({
    userName: this.fb.control(''),
    password: this.fb.control('')
  });

  constructor(private fb: FormBuilder, private authService: AuthService, private router: Router) {

  }


}
