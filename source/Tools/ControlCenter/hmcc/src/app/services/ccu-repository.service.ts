import { Injectable } from '@angular/core';
import {HttpClient} from "@angular/common/http";
import {firstValueFrom, Observable} from "rxjs";
import {CcuModel} from "./ccu-model";

@Injectable({
  providedIn: 'root'
})
export class CcuRepositoryService {

  apiBaseUrl = 'http://localhost:5001';

  constructor(private httpClient: HttpClient) { }

  getAllCcu() : Observable<CcuModel[]>{
    return this.httpClient.get<CcuModel[]>(`${this.apiBaseUrl}/ccu`);
  }

  getCcu(id: number) : Observable<CcuModel>{
    return this.httpClient.get<CcuModel>(`${this.apiBaseUrl}/ccu/${id}`);
  }

  getCcuByUrl(url: string) : Observable<CcuModel>{
    return this.httpClient.get<CcuModel>(`${this.apiBaseUrl}/ccu/byurl/${url}`);
  }

  async addCcu(ccu: CcuModel) {
    const returned = await  firstValueFrom(this.httpClient.post<CcuModel>(`${this.apiBaseUrl}/ccu`, ccu));

    ccu.id = returned.id;
  }

  removeCcu(ccu: CcuModel) {
    return this.httpClient.delete(`${this.apiBaseUrl}/ccu/${ccu.id}`);
  }
}
