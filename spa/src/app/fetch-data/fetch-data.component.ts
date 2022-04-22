import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-fetch-data',
  templateUrl: './fetch-data.component.html',
  styleUrls: ['./fetch-data.component.css']
})
export class FetchDataComponent {
  public forecasts: WeatherForecast[] = [];

  // TODO: set BASE_URL globally
  constructor(http: HttpClient ) {
    var baseUrl = environment.baseUrl;

    http.get<WeatherForecast[]>(baseUrl + 'weatherforecast/get').subscribe({
      next: (result) => this.forecasts = result,
      error: (error) => console.error(error)
    });
  }
}

interface WeatherForecast {
  date: string;
  temperatureC: number;
  temperatureF: number;
  summary: string;
}
