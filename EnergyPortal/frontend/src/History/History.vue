<template>
  <div>
    <v-row>
      <v-col cols="6">
        <v-switch
            color="primary"
            v-model="showCosts"
            label="Toon kosten"
        ></v-switch>
      </v-col>
      <v-col v-if="selectedRangeText !== null" cols="6">
        <v-select
            variant="outlined"
            class="float-right"
            v-model="selectedRangeText"
            :items="defaultRangeOptions.map(d => d.text)"
            label="Bereik"
            style="width: 15rem;"
        ></v-select>
      </v-col>
    </v-row>
    <v-row v-if="customEnabled">
      <v-col cols="6">
        <v-date-picker 
            width="100%"
            color="primary"
            v-model="customFromDate"
            :max="customToDate"
        ></v-date-picker>
      </v-col>
      <v-col cols="6">
        <v-date-picker 
            width="100%"
            color="primary"
            v-model="customToDate"
            :min="customFromDate"
        ></v-date-picker>
      </v-col>
    </v-row>
    <v-row v-if="customEnabled">
      <v-col cols="6">
        <v-switch
            color="primary"
            v-model="skipTimeSpanCheck"
            label="Negeer waarschuwing"
        ></v-switch>
      </v-col>
      <v-col v-if="selectedRangeText !== null" cols="6">
        <v-select
            variant="outlined"
            class="float-right"
            v-model="customGroupBy"
            :items="Object.values(groupByOptions)"
            label="Bereik"
            style="width: 15rem;"
        ></v-select>
          <!--        <b-dropdown variant="outline-secondary" v-if="customEnabled" :text="groupByOptions[oldGroupBy]" right class="float-right mb-3 mr-2">-->
          <!--          <b-dropdown-item-->
          <!--              v-for="(text, groupBy) in groupByOptions"-->
          <!--              :key="text"-->
          <!--              @click="selectedRange.groupBy = groupBy"-->
          <!--              href="#"-->
          <!--          >-->
          <!--            {{ text }}-->
          <!--          </b-dropdown-item>-->
          <!--        </b-dropdown>-->
      </v-col>
    </v-row>
    <v-row>
      <v-col cols="12">
        <v-card elevation="3" class="pa-2">
          <div class="text-subtitle-1 text-center text-grey-darken-2">
            Elektriciteit
          </div>
          <apexchart width="100%" height="400" :series="chartData.series" :options="chartData.chartOptions"></apexchart>
        </v-card>
      </v-col>
      <v-col cols="12">
        <v-card elevation="3" class="pa-2">
          <div class="text-subtitle-1 text-center text-grey-darken-2">
            Gas
          </div>
          <apexchart width="100%" height="400" :series="gasChartData.series" :options="gasChartData.chartOptions"></apexchart>
        </v-card>
      </v-col>
    </v-row>

    <v-sonner position="bottom-right" />
  </div>
</template>

<script>
import Axios from "axios"
import moment from "moment-timezone";
import deepCopy from "../deepCopy.js";
import {VSonner, toast } from "vuetify-sonner";

export default {
  name: "",
  components: {VSonner},
  data: () => ({
    messageText: '',
    messageVariant: 'info',
    showMessage: false,
    timeout: 5000,
    showCosts: false,
    skipTimeSpanCheck: false,
    defaultRangeOptions: [
      {
        text: 'Afgelopen 24 uur',
        groupBy: 'hours',
        amount: 24
      },
      {
        text: 'Laatste 7 dagen',
        groupBy: 'days',
        amount: 6
      },
      {
        text: 'Afgelopen maand',
        groupBy: 'days',
        amount: 30
      },
      {
        text: 'Afgelopen jaar',
        groupBy: 'months',
        amount: 12
      },
      {
        text: 'Afgelopen 5 jaar',
        groupBy: 'years',
        amount: 5
      },
      {
        text: 'Selecteer bereik',
        groupBy: null
      }
    ],
    selectedRangeText: 'Afgelopen 24 uur',
    selectedRange: null,
    oldGroupBy: null,
    groupByOptions: {
      tenseconds: 'Per ten seconden',
      minutes: 'Per minuut',
      hours: 'Per uur',
      days: 'Per dag',
      weeks: 'Per week',
      months: 'Per maand',
      years: 'Per jaar',
    },
    chartData: {
      chartOptions: {
        colors: ['#007bff', '#ffc107', '#28a745', '#dc3545'],
        chart: {
          type: 'bar',
          stacked: false,
          toolbar: {
            show: true
          },
          zoom: {
            enabled: true
          }
        },
        plotOptions: {
          bar: {
            horizontal: false,
            columnWidth: '80%',
            endingShape: 'rounded'
          },
        },
        dataLabels: {
          enabled: false
        },
        stroke: {
          show: true,
          width: 5,
          colors: ['transparent']
        },
        xaxis: {
          type: 'datetime',
          labels: {
            formatter: null
          }
        },
        fill: {
          opacity: 1
        },
        tooltip: {
          x: {
            format: 'yyyy-MM-dd',
            timeZone: 'UTC'
          }
        },
        // fill: {
        //   type: "gradient",
        //   gradient: {
        //     shadeIntensity: 1,
        //     opacityFrom: 0.5,
        //     opacityTo: 0.8,
        //     stops: [0, 100, 100]
        //   }
        // }
      },
      series: [
        {
          data: [],
          name: ''
        }
      ]
    },
    gasChartData: {
      chartOptions: {
        colors: ['#dc3545'],
        chart: {
          type: 'bar',
          stacked: false,
          toolbar: {
            show: true
          },
          zoom: {
            enabled: true
          }
        },
        plotOptions: {
          bar: {
            horizontal: false,
            columnWidth: '80%',
            endingShape: 'rounded'
          },
        },
        dataLabels: {
          enabled: false
        },
        stroke: {
          show: true,
          width: 5,
          colors: ['transparent']
        },
        xaxis: {
          type: 'datetime',
          labels: {
            formatter: null
          }
        },
        fill: {
          opacity: 1
        },
        tooltip: {
          x: {
            format: 'yyyy-MM-dd',
            timeZone: 'UTC'
          }
        },
      },
      series: [
        {
          data: [],
          name: ''
        }
      ]
    },
    datasets: {
      usage: {
        label: 'Verbruik in kWh',
        backgroundColor: '#007bff',
        data: []
      },
      solar: {
        label: 'Opwekking in kWh',
        backgroundColor: '#ffc107',
        data: []
      },
      redelivery: {
        label: 'Teruglevering in kWh',
        backgroundColor: '#28a745',
        data: []
      },
      intake: {
        label: 'Opname in kWh',
        backgroundColor: '#dc3545',
        data: []
      },
      usageCosts: {
        label: 'Totaal in €',
        backgroundColor: '#007bff',
        data: []
      },
      intakeCosts: {
        label: 'Opname in €',
        backgroundColor: '#dc3545',
        data: []
      },
      redeliveryCosts: {
        label: 'Teruglevering in €',
        backgroundColor: '#28a745',
        data: []
      },
      gas: {
        label: 'Gas in M³',
        backgroundColor: '#dc3545',
        data: []
      },
      gasCosts: {
        label: 'Gas in €',
        backgroundColor: '#dc3545',
        data: []
      }
    },
    options: {
      responsive: true,
      maintainAspectRatio: false,
      scales: {
        yAxes: [{
          display: true,
          ticks: {
            suggestedMin: 0,
            suggestedMax: 1
          }
        }]
      }
    },
    customFromDate: '',
    customToDate: '',
    customGroupBy: 'Per dag',
    fromDate: '',
    fromTime: '00:00:00',
    toDate: '',
    toTime: '00:00:00',
    settings: {
      solarSystem: false
    },
    loading: true
  }),
  async mounted () {
    this.chartData.chartOptions.xaxis.labels.formatter = v => this.formatEpoch(v, this.chartData.chartOptions);
    this.gasChartData.chartOptions.xaxis.labels.formatter = v => this.formatEpoch(v, this.chartData.chartOptions);
    
    await this.getUserSettings();

    // This will eventually trigger fetchHistory by the watcher
    this.selectedRange = this.defaultRangeOptions.find(r => r.text === 'Laatste 7 dagen');
  },
  methods: {
    formatEpoch(epoch, chartData) {
      const d = new Date(epoch);
      let year = d.toLocaleString([], { year: 'numeric', timeZone: chartData.tooltip.x.timeZone});
      let month = d.toLocaleString([], { month: '2-digit', timeZone: chartData.tooltip.x.timeZone});
      let day = d.toLocaleString([], { day: '2-digit', timeZone: chartData.tooltip.x.timeZone});
      let hours = d.toLocaleString([], { hour: '2-digit', hour12: false, timeZone: chartData.tooltip.x.timeZone});
      let minutes = d.toLocaleString([], { minute: '2-digit', timeZone: chartData.tooltip.x.timeZone});
      let seconds = d.toLocaleString([], { second: '2-digit', timeZone: chartData.tooltip.x.timeZone});
      const daysOfWeek = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];

      switch(chartData.tooltip.x.format) {
        case 'HH:mm':
          return `${hours}:${minutes}`;
        case 'HH:mm:ss':
          return `${hours}:${minutes}:${seconds}`;
        case 'MM-dd HH:mm':
          return `${month}-${day} ${hours}:${minutes}`;
        case 'yyyy-MM-dd (dddd)':
          const dayName = daysOfWeek[d.getUTCDay()]; // Get the day name
          return `${year}-${month}-${day} (${dayName})`;
        case 'yyyy-MM-dd':
          return `${year}-${month}-${day}`;
        case 'yyyy-MM':
          return `${year}-${month}`;
        case 'yyyy':
          return `${year}`;
      }
    },
    async getUserSettings() {
      try {
        let response = await Axios.get('webapi/v3/settings');
        this.settings = response.data;
      } catch (e) {
        console.error(e);
        this.makeToast('Failed to fetch user settings.', 'danger');
      }
    },
    async fetchHistory(populateCharts = true) {
      try {
        let config = {
          params: {
            from: this.from ? this.from : moment().substract(30, 'days').format(),
            to: this.to ? this.to : moment().format(),
            showRealLast: true,
            skipTimeSpanCheck: this.skipTimeSpanCheck
          }
        }

        let response = await Axios.get(`/webapi/v3/metrics/${this.selectedRange.groupBy}`, config);

        this.chartData.chartOptions.xaxis.categories = response.data.timestamps;
        this.chartData.chartOptions.tooltip.x.format = response.data.format;
        this.chartData.chartOptions.tooltip.x.timeZone = response.data.userTimeZone;
        this.gasChartData.chartOptions.xaxis.categories = response.data.timestamps;
        this.gasChartData.chartOptions.tooltip.x.format = response.data.format;
        this.gasChartData.chartOptions.tooltip.x.timeZone = response.data.userTimeZone;
        
        this.datasets.usage.data = response.data.usage;
        this.datasets.solar.data = response.data.solar;
        this.datasets.redelivery.data = response.data.redelivery;
        this.datasets.intake.data = response.data.intake;
        this.datasets.usageCosts.data = response.data.usageCosts;
        this.datasets.intakeCosts.data = response.data.intakeCosts;
        this.datasets.redeliveryCosts.data = response.data.redeliveryCosts;
        this.datasets.intake.data = response.data.intake;
        this.datasets.gas.data = response.data.gas;
        this.datasets.gasCosts.data = response.data.gasCosts;

        if (populateCharts) {
          this.populateChartsWithCorrectDatasets();
        }
      } catch (e) {
        console.error(e);

        if (e.response.status === 400)
          this.makeToast(e.response.data, 'warning');
        else
          this.makeToast('An error occured while fetching the history', 'danger')
      }
    },
    setSelectedRange(range) {
      this.selectedRange = deepCopy(range);
    },
    makeToast(message, variant = null) {
      toast(message, {
        cardProps: {
          outlined: true,
          color: variant
        }
      });
      this.messageText = message;
      this.showMessage = true;
    },
    populateChartsWithCorrectDatasets() {
      this.gasChartData.series = [];
      this.chartData.series = [];

      if (this.showCosts) {
        let gasSeries = {
          data: this.datasets.gasCosts.data,
          name: this.datasets.gasCosts.label
        }
        this.gasChartData.series.push(gasSeries);

        let usageSeries = {
          data: this.datasets.usageCosts.data,
          name: this.datasets.usageCosts.label
        }
        this.chartData.series.push(usageSeries);

        if (this.settings.solarSystem) {
          let intakeSeries = {
            data: this.datasets.intakeCosts.data,
            name: this.datasets.intakeCosts.label
          }
          let redeliverySeries = {
            data: this.datasets.redeliveryCosts.data,
            name: this.datasets.redeliveryCosts.label
          }
          
          this.chartData.series.push(intakeSeries);
          this.chartData.series.push(redeliverySeries);
        }
      } else {
        let gasSeries = {
          data: this.datasets.gas.data,
          name: this.datasets.gas.label
        }
        this.gasChartData.series.push(gasSeries);
        
        let usageSeries = {
          data: this.datasets.usage.data,
          name: this.datasets.usage.label
        }
        
        this.chartData.series.push(usageSeries);

        if (this.settings.solarSystem) {
          let solarSeries = {
            data: this.datasets.solar.data,
            name: this.datasets.solar.label
          }
          let redeliverySeries = {
            data: this.datasets.redelivery.data,
            name: this.datasets.redelivery.label
          }
          let intakeSeries = {
            data: this.datasets.intake.data,
            name: this.datasets.intake.label
          }
          
          this.chartData.series.push(solarSeries);
          this.chartData.series.push(redeliverySeries);
          this.chartData.series.push(intakeSeries);
        }
      }
    }
  },
  computed:{
    from() {
      if (this.fromDate.length < 1)
        return null;

      if (this.fromTime.length < 1)
        return null;

      return `${this.fromDate}T${this.fromTime}${this.offset}`;
    },
    to() {
      if (this.toDate.length < 1)
        return null;

      if (this.toTime.length < 1)
        return null;

      return `${this.toDate}T${this.toTime}${this.offset}`;
    },
    offset() {
      let timeZoneId = this.settings.timeZoneId || "Europe/Amsterdam";

      let nowString = moment().tz(timeZoneId).format().split('T')[1];
      return nowString.substring(8);
    },
    customEnabled() {
      return (this.selectedRange && this.selectedRange.text === 'Selecteer bereik') || false;
    },
    getFromMoment() {
      let from = moment();

      switch (this.selectedRange.groupBy) {
        case 'minutes':
          from.subtract(this.selectedRange.amount, 'minutes');
          break;
        case 'hours':
          from.subtract(this.selectedRange.amount, 'hours');
          break;
        case 'days':
        case 'weeks':
          from.subtract(this.selectedRange.amount, 'days').startOf('day');
          break;
        case 'months':
          from = from.subtract(this.selectedRange.amount, 'months').startOf('month');
          from.add(1, 'months');
          break;
        case 'years':
          from.subtract(this.selectedRange.amount, 'years').startOf('year');
          from.add(1, 'years');
          break;
      }

      return from;
    }
  },
  watch: {
    customFromDate() {
      if (!this.customEnabled)
        return;

      let year = this.customFromDate.toLocaleString([], { year: 'numeric' });
      let month = this.customFromDate.toLocaleString([], { month: '2-digit' });
      let day = this.customFromDate.toLocaleString([], { day: '2-digit' });
      
      this.selectedRange.groupBy = Object.keys(this.groupByOptions).find(key => this.groupByOptions[key] === this.customGroupBy)
      
      this.fromDate = `${year}-${month}-${day}`;
      this.fromTime = '00:00:00';

      this.fetchHistory();
    },
    customToDate() {
      if (!this.customEnabled)
        return;

      let year = this.customToDate.toLocaleString([], { year: 'numeric' });
      let month = this.customToDate.toLocaleString([], { month: '2-digit' });
      let day = this.customToDate.toLocaleString([], { day: '2-digit' });

      this.toDate = `${year}-${month}-${day}`;
      this.toTime = '23:59:59';
      this.selectedRange.groupBy = Object.keys(this.groupByOptions).find(key => this.groupByOptions[key] === this.customGroupBy)

      this.fetchHistory();
    },
    customGroupBy() {
      if (!this.customEnabled)
        return;

      this.selectedRange.groupBy = Object.keys(this.groupByOptions).find(key => this.groupByOptions[key] === this.customGroupBy)
      this.fetchHistory();
    },
    skipTimeSpanCheck() {
      this.fetchHistory();
    },
    selectedRangeText(value) {
      this.selectedRange = this.defaultRangeOptions.find(r => r.text === value);
    },
    selectedRange: {
      async handler(range) {

        if (range !== null)
          console.log(`Range selection changed! new (${range.groupBy}), old (${this.oldGroupBy})`);

        if (this.customEnabled) {
          if (range.groupBy === null){
            range.groupBy = this.oldGroupBy;
            return;
          }

          if (range.groupBy !== this.oldGroupBy)
            await this.fetchHistory();

          this.oldGroupBy = range.groupBy;
          return;
        } else {
          this.toDate = '';
          this.toTime = '';
        }

        let from = this.getFromMoment.format().split('T');
        this.fromDate = from[0];
        this.fromTime = from[1].split('+')[0];

        await this.fetchHistory();
        this.oldGroupBy = range.groupBy;
      },
      deep: true
    },
    showCosts(value) {
      let costsSettingsNotFullySet = this.settings.highUsagePricePerKwh === 0 ||
          this.settings.lowUsagePricePerKwh === 0 ||
          this.settings.highRedeliveryPricePerKwh === 0 ||
          this.settings.lowRedeliveryPricePerKwh === 0 ||
          this.settings.gasPrice === 0 ||
          this.settings.electricityDeliveryPricePerMonth === 0 ||
          this.settings.gasDeliveryPricePerMonth === 0;

      if (value && costsSettingsNotFullySet) {
        this.makeToast('Niet alle prijzen zijn bekend. Stel deze in onder Instellingen', 'warning');
      }

      this.populateChartsWithCorrectDatasets();
    }
  }
}
</script>

<style scoped>

</style>