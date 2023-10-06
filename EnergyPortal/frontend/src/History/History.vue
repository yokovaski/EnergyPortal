<template>
  <div>
    <div class="row">
      <div class="col-6">
        <b-form-checkbox v-model="showCosts" name="check-button-costs" switch>
          Toon kosten
        </b-form-checkbox>
      </div>
      <div v-if="selectedRange !== null" class="col-6">
        <b-dropdown :text="selectedRange.text" right class="float-right mb-3">
          <b-dropdown-item
              v-for="range in defaultRangeOptions"
              :key="range.text"
              @click="setSelectedRange(range)"
              href="#"
          >
            {{ range.text }}
          </b-dropdown-item>
        </b-dropdown>

        <b-dropdown variant="outline-secondary" v-if="customEnabled" :text="groupByOptions[oldGroupBy]" right class="float-right mb-3 mr-2">
          <b-dropdown-item
              v-for="(text, groupBy) in groupByOptions"
              :key="text"
              @click="selectedRange.groupBy = groupBy"
              href="#"
          >
            {{ text }}
          </b-dropdown-item>
        </b-dropdown>
      </div>
      <div v-if="customEnabled" class="col-12">
        <div class="row">
          <div class="col-12 col-sm-6">
            <b-form-group
                id="from"
                label="From"
                label-for="from-datepicker"
            >
              <b-form-datepicker
                  id="from-datepicker"
                  class="mb-1"
                  v-model="fromDate"
              ></b-form-datepicker>
              <b-form-timepicker
                  hourCycle="h23"
                  reset-button
                  v-model="fromTime"
              ></b-form-timepicker>
            </b-form-group>
          </div>
          <div class="col-12 col-sm-6">
            <b-form-group
                id="to"
                label="To"
                label-for="from-datepicker"
            >
              <b-form-datepicker
                  id="to-datepicker"
                  class="mb-1"
                  v-model="toDate"
              ></b-form-datepicker>
              <b-form-timepicker
                  now-button
                  reset-button
                  v-model="toTime"
              ></b-form-timepicker>
            </b-form-group>
          </div>
        </div>
      </div>
      <div class="col-12">
        <div class="card shadow-sm mb-3">
          <div class="card-body">
            <bar-chart ref="electricityChart" :chart-data="chartData" :options="options"></bar-chart>
          </div>
        </div>
      </div>
      <div class="col-12">
        <div class="card shadow-sm">
          <div class="card-body">
            <bar-chart ref="gasChart" :chart-data="gasChartData" :options="options"></bar-chart>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import Axios from "axios"
import BarChart from "../Charts/BarChart.vue"
import moment from "moment-timezone";
import deepCopy from "../deepCopy.js";

export default {
  name: "",
  components: { BarChart},
  data: () => ({
    showCosts: false,
    defaultRangeOptions: [
      {
        text: 'Last day',
        groupBy: 'hours',
        amount: 24
      },
      {
        text: 'Last week',
        groupBy: 'days',
        amount: 6
      },
      {
        text: 'Last month',
        groupBy: 'days',
        amount: 30
      },
      {
        text: 'Last year',
        groupBy: 'months',
        amount: 12
      },
      {
        text: 'Last 5 years',
        groupBy: 'years',
        amount: 5
      },
      {
        text: 'Select range',
        groupBy: null
      }
    ],
    selectedRange: null,
    oldGroupBy: null,
    groupByOptions: {
      tenseconds: 'Per ten seconds',
      minutes: 'Per minute',
      hours: 'Per hour',
      days: 'Per day',
      weeks: 'Per week',
      months: 'Per month',
      years: 'Per year',
    },
    chartData: {
      labels: [],
      datasets: []
    },
    gasChartData: {
      labels: [],
      datasets: []
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
    fromDate: '',
    fromTime: '00:00:00',
    toDate: '',
    toTime: '',
    settings: {
      solarSystem: false
    },
    loading: true
  }),
  async mounted () {
    // Can't fetch history directly here because moment() does not yet exist somehow..
    // await Promise.all([
    //   this.getUserSettings(), 
    //   this.fetchHistory()]
    // );

    await this.getUserSettings();

    // This will eventually trigger fetchHistory by the watcher
    this.selectedRange = this.defaultRangeOptions.find(r => r.text === 'Last week');
  },
  methods: {
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
            to: this.to ? this.to : moment().format()
          }
        }

        let response = await Axios.get(`/webapi/v3/metrics/${this.selectedRange.groupBy}`, config);

        this.chartData.labels = response.data.timestamps;
        this.gasChartData.labels = response.data.timestamps;
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
      this.$bvToast.toast(message, {
        variant: variant,
        solid: true
      })
    },
    populateChartsWithCorrectDatasets() {
      this.chartData.datasets = [];
      this.gasChartData.datasets = [];

      if (this.showCosts) {
        this.chartData.datasets.push(this.datasets.usageCosts);
        this.gasChartData.datasets.push(this.datasets.gasCosts);

        if (this.settings.solarSystem) {
          this.chartData.datasets.push(this.datasets.intakeCosts);
          this.chartData.datasets.push(this.datasets.redeliveryCosts);
        }
      } else {
        this.chartData.datasets.push(this.datasets.usage);
        this.gasChartData.datasets.push(this.datasets.gas);

        if (this.settings.solarSystem) {
          this.chartData.datasets.push(this.datasets.solar);
          this.chartData.datasets.push(this.datasets.redelivery);
          this.chartData.datasets.push(this.datasets.intake);
        }
      }

      this.$refs.electricityChart.update();
      this.$refs.gasChart.update();
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
      return (this.selectedRange && this.selectedRange.text === 'Select range') || false;
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
    fromDate() {
      if (!this.customEnabled)
        return;

      if (this.fromTime.length > 0)
        this.fetchHistory();
    },
    fromTime() {
      if (!this.customEnabled)
        return;

      if (this.fromDate.length > 0)
        this.fetchHistory();
    },
    toDate() {
      if (!this.customEnabled)
        return;

      if (this.toTime.length > 0)
        this.fetchHistory();
    },
    toTime() {
      if (!this.customEnabled)
        return;

      if (this.toDate.length > 0)
        this.fetchHistory();
    },
    selectedRange: {
      async handler(range) {
        // if (this.loading) {
        //   return;
        // }

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

        // if (subtractFormat === 'hours' || subtractFormat === 'minutes')
        //     this.fromTime = from[1].split('+')[0];
        // else
        //     this.fromTime = '00:00:00';

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