import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpService } from './http.service';

export interface RecentActivity {
  type: string;
  userName: string;
  description: string;
  amount?: string;
  time: string;
  avatarUrl?: string;
  badge?: {
    text: string;
    color: string;
  };
}

@Injectable({
  providedIn: 'root',
})
export class ActivityService {
  constructor(private http: HttpService) {}

  getRecent(limit: number = 5): Observable<RecentActivity[]> {
    return this.http.get<RecentActivity[]>(`statistics/recent-activities?limit=${limit}`);
  }
}
