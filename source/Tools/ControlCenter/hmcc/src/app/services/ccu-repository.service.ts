import { Injectable } from '@angular/core';
import {HttpClient} from "@angular/common/http";
import {firstValueFrom, Observable, tap} from "rxjs";
import {CcuModel} from "./ccu-model";
import {getLocaleFirstDayOfWeek} from "@angular/common";

@Injectable({
  providedIn: 'root'
})
export class CcuRepositoryService {

  apiBaseUrl = 'http://localhost:5001';

  constructor(private httpClient: HttpClient) {
  }

  getAllCcu(): Observable<CcuModel[]> {
    return this.httpClient.get<CcuModel[]>(`${this.apiBaseUrl}/ccu`);
  }

  getCcu(id: number): Observable<CcuModel> {
    return this.httpClient.get<CcuModel>(`${this.apiBaseUrl}/ccu/${id}`);
  }

  getCcuByUrl(url: string): Observable<CcuModel> {
    return this.httpClient.get<CcuModel>(`${this.apiBaseUrl}/ccu/byurl/${url}`);
  }

  addCcu(ccu: CcuModel) {
    return this.httpClient.post<CcuModel>(`${this.apiBaseUrl}/ccu`, ccu)
      .pipe(tap(returned => ccu.id = returned.id));
  }

  removeCcu(ccu: CcuModel) {
    return this.httpClient.delete(`${this.apiBaseUrl}/ccu/${ccu.id}`);
  }
}
