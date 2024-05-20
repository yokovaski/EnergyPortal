<template>
  <div>
    <v-row
        v-if="Object.keys(energyNameMapping).length > 0"
    >
      <v-col
          v-for="(electricityType, type) in latestEnergyValues"
          :key="`single-stat-${type}`"
          cols="12"
          sm="3"
      >
        <v-card
            elevation="3"
            class="pa-6"
        >
          <div
              class="text-subtitle-2 text-center font-weight-regular"
              :style="{color: electricityType.textColor}"
              v-text="energyNameMapping[type]"
          ></div>
          <div
              class="text-h4 text-center mt-2 mb-2 font-weight-bold text-grey-darken-2"
              v-text="electricityType.value"
          ></div>
          <div class="text-subtitle-1 text-center font-weight-light text-grey-darken-1">
            kW
          </div>
        </v-card>
      </v-col>
    </v-row>
    <v-row no-gutters>
      <v-col cols="12">
        <div class="pb-3"></div>
      </v-col>
    </v-row>
    <v-row no-gutters>
      <v-col
          cols="12"
          md="8"
      >
        <v-row no-gutters>
          <v-col>
            <v-row
                no-gutters
                v-for="chartName in chartNames"
                :key="`chart-${chartName}`"
            >
              <v-col
                  cols="12"
                  class="pt-3 pb-3"
                  :class="{'pr-3': $vuetify.display.mdAndUp}"
              >
                <electricity-chart
                    v-if="electricityCharts[chartName].chartData.chartOptions.dataLabels"
                    ref="electricityChart"
                    :chart-name="electricityCharts[chartName].chartName"
                    :chart-data="electricityCharts[chartName].chartData"
                    :active-range="chartRange"
                    v-on:change-range="setGraphRange($event)"
                ></electricity-chart>
              </v-col>
            </v-row>
          </v-col>
        </v-row>
      </v-col>
      <v-col
          cols="12"
          md="4"
      >
        <v-row
            no-gutters
        >
          <v-col
              cols="12"
              class="pt-3 pb-3"
              :class="{'pl-3': $vuetify.display.mdAndUp}"
          >
            <v-card
                class="pa-5"
                elevation="3"
            >
              <div
                  class="text-subtitle-2 text-center text-grey-darken-2"
                  v-text="'Opname vandaag'"
              ></div>
              <div
                  class="text-h4 text-center font-weight-bold text-grey-darken-2 mt-2 mb-2"
                  v-text="round(energyToday.usageTotalHigh + energyToday.usageTotalLow)"
              ></div>
              <div class="text-subtitle-1 text-center text-grey-darken-2">
                kWh
              </div>
            </v-card>
          </v-col>
        </v-row>
        <v-row
            no-gutters
        >
          <v-col
              cols="12"
              class="pt-3 pb-3 pl-3"
          >
            <v-card
                class="pa-5"
                elevation="3"
            >
              <div
                  class="text-subtitle-2 text-center text-grey-darken-2"
                  v-text="'Teruglevering vandaag'"
              ></div>
              <div
                  class="text-h4 text-center font-weight-bold text-grey-darken-2 mt-2 mb-2"
                  v-text="round(energyToday.redeliveryTotalHigh + energyToday.redeliveryTotalLow)"
              ></div>
              <div class="text-subtitle-1 text-center text-grey-darken-2">
                kWh
              </div>
            </v-card>
          </v-col>
        </v-row>
        <v-row
            no-gutters
        >
          <v-col
              cols="12"
              class="pt-3 pb-3 pl-3"
          >
            <v-card
                class="pa-5"
                elevation="3"
            >
              <div
                  class="text-subtitle-2 text-center text-grey-darken-2"
                  v-text="'Opwekking vandaag'"
              ></div>
              <div
                  class="text-h4 text-center font-weight-bold text-grey-darken-2 mt-2 mb-2"
                  v-text="energyToday.totalSolar"
              ></div>
              <div class="text-subtitle-1 text-center text-grey-darken-2">
                kWh
              </div>
            </v-card>
          </v-col>
        </v-row>
        <v-row
            no-gutters
        >
          <v-col
              cols="12"
              class="pt-3 pb-3 pl-3"
          >
            <v-card
                class="pa-5"
                elevation="3"
            >
              <div
                  class="text-subtitle-2 text-center text-grey-darken-2"
                  v-text="'Gasverbruik vandaag'"
              ></div>
              <div
                  class="text-h4 text-center font-weight-bold text-grey-darken-2 mt-2 mb-2"
                  v-text="energyToday.totalGas"
              ></div>
              <div class="text-subtitle-1 text-center text-grey-darken-2">
                M³
              </div>
            </v-card>
          </v-col>
        </v-row>
        <v-row
            no-gutters
        >
          <v-col
              cols="12"
              class="pt-3 pb-3 pl-3"
          >
            <v-card
                class="pa-5"
                elevation="3"
            >
              <div
                  class="text-subtitle-2 text-center text-grey-darken-2"
                  v-text="'Meterstanden'"
              ></div>
              <v-table>
                <tbody>
                <tr
                    v-for="total in totals"
                    :key="total.title"
                >
                  <td
                      class="text--secondary font-weight-bold text-grey-darken-2"
                      v-text="total.title"
                  ></td>
                  <td
                      class="text--secondary"
                      v-text="total.value"
                  ></td>
                </tr>
                </tbody>
              </v-table>
            </v-card>
          </v-col>
        </v-row>
      </v-col>
    </v-row>
  </div>
</template>

<script>
// @ is an alias to /src
import ElectricityChart from "./ElectricityChart.vue";
import Axios from "axios";
let initialElectricityCharts = {
  usage: {
    index: 0,
    chartName: "Verbruik",
    backEndName: "usage",
    borderColor: "#007bff",
    backgroundColor: "rgba(0,137,255,0.26)"
  },
  solar: {
    index: 0,
    chartName: "Opwekking",
    backEndName: "solar",
    borderColor: "#ffc107",
    backgroundColor: "rgba(255,193,7,0.26)"
  },
  redelivery: {
    index: 0,
    chartName: "Teruglevering",
    backEndName: "redelivery",
    borderColor: "#28a745",
    backgroundColor: "rgba(40,167,69,0.25)"
  },
  intake: {
    index: 0,
    chartName: "Opname",
    backEndName: "intake",
    borderColor: "#dc3545",
    backgroundColor: "rgba(220,53,69,0.26)"
  },
};
export default {
  name: "HomeDashboard",
  components: { ElectricityChart },
  data: () => ({
    chartNames: [],
    electricityCharts: {},
    energyNameMapping: {},
    electricityTypes:[
      {
        title: "Verbruik",
        class: "blue--text",
        value: "1,583"
      },
      {
        title: "Opwekking",
        class: "orange--text",
        value: "0,830"
      },
      {
        title: "Teruglevering",
        class: "green--text",
        value: "0,000"
      },
      {
        title: "Opname",
        class: "red--text",
        value: "0,753"
      },
    ],
    latestEnergyValues: {
      usage: {
        textColor: "#007bff",
        value: "N/A"
      },
      solar: {
        textColor: "#ffc107",
        value: "N/A"
      },
      redelivery: {
        textColor: "#28a745",
        value: "N/A"
      },
      intake: {
        textColor: "#dc3545",
        value: "N/A"
      },
    },
    energyToday:{
      usageTotalHigh: 0,
      usageTotalLow: 0,
      redeliveryTotalHigh: 0,
      redeliveryTotalLow: 0,
      totalSolar: 0,
      totalGas: 0
    },
    totals: {
      usageTotalHigh: {
        title: "Opname hoog",
        value: 0
      },
      redeliveryTotalHigh: {
        title: "Opname laag",
        value: 0
      },
      usageTotalLow: {
        title: "Levering hoog",
        value: 0
      },
      redeliveryTotalLow: {
        title: "Levering laag",
        value: 0
      },
      usageGasTotal: {
        title: "Gas",
        value: 0
      },
    },
    chartRange: "day",
    lastRefresh: null,
    timer: null,
    timeLookup: {}
  }),
  async mounted () {
    this.initCharts();
    await Promise.all([
      this.fetchChartData(),
      this.fetchTotalsToday(),
      this.fetchTotals()
    ]);
    
    await this.fetchLast();

    let self = this;
    this.timer = setInterval(function () {
      self.fetchLast()
    }, 10000)
  },
  beforeDestroy() {
    clearInterval(this.timer);
  },
  methods: {
    initCharts() {
      for (const chart of Object.values(initialElectricityCharts)) {
        this.electricityCharts[chart.chartName] = {
          chartName: chart.chartName,
          chartData: this.buildChartData(chart.chartName, [], [], chart.backgroundColor, chart.borderColor, 'HH:mm'),
          backEndName: chart.backEndName
        };

        this.chartNames.push(chart.chartName);
        this.energyNameMapping[chart.backEndName] = chart.chartName;
      }
    },
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
    buildChartData(dataName, data, labels, backgroundColor, borderColor, format) {
      let chartData = {
        chartOptions: {
          colors: [borderColor],
          chart: {
            height: 350,
            type: 'area',
            zoom: {
              enabled: false
            }
          },
          dataLabels: {
            enabled: false
          },
          stroke: {
            curve: 'smooth'
          },
          xaxis: {
            type: 'datetime',
            labels: {
              formatter: null
            }
          },
          tooltip: {
            x: {
              format: format,
              timeZone: 'UTC'
            }
          },
          fill: {
            type: "gradient",
            gradient: {
              shadeIntensity: 1,
              opacityFrom: 0.5,
              opacityTo: 0.8,
              stops: [0, 100, 100]
            }
          }
        },
        series: [{
          name: dataName,
          data: data
        }]
      }

      chartData.chartOptions.xaxis.labels.formatter = v => this.formatEpoch(v, chartData.chartOptions);
      
      return chartData;
    },
    async setGraphRange(range) {
      this.chartRange = range;
      await this.fetchChartData();
    },
    async fetchChartData() {
      try {
        let response = await Axios.get(this.chartUri, this.chartConfig);
        let data = response.data;
        for (const chartName of this.chartNames) {
          let backEndName = this.electricityCharts[chartName].backEndName;
          let chartData = data[backEndName];
          if (chartData === undefined)
            continue;
          
          this.electricityCharts[chartName].chartData.series[0].data = chartData;
          this.electricityCharts[chartName].chartData.chartOptions.xaxis.categories = data.timestamps;
          this.electricityCharts[chartName].chartData.chartOptions.tooltip.x.format = data.format;
          this.electricityCharts[chartName].chartData.chartOptions.tooltip.x.timeZone = data.userTimeZone;
          
          console.log(`Fetched data for ${chartName} with format ${data.format}`);
          this.timeLookup[data.format] = chartData
              .map((epochAndValue, index) => ({
                epoch: epochAndValue[0],
                value: data.timestamps[index]
              }))
              .reduce((dict, current) => {
                dict[current.epoch] = current.value;
                return dict;
              }, {});
        }
        this.lastRefresh = data.queryTimestamp;
      } catch (e) {
        console.error(e);
      }
    },
    async fetchLast() {
      if (this.lastRefresh === null)
        return;
      try {
        let config = {
          params: {
            from: this.lastRefresh,
            showRealLast: true
          }
        };
        let response = await Axios.get('/webapi/v3/metrics/tenseconds', config);
        let data = response.data;
        this.lastRefresh = data.queryTimestamp;
        if (data.timestamps.length > 0) {
          let index = data.timestamps.length - 1;
          for (let energyType of Object.keys(this.latestEnergyValues)) {
            this.latestEnergyValues[energyType].value = data[energyType][index][1];
          }
        }
      } catch (e) {
        console.error(e);
      }
    },
    async fetchTotals() {
      try {
        let response = await Axios.get('/webapi/v3/energy/totals')
        for (const [key, value] of Object.entries(response.data)) {
          if (key in this.totals)
            this.totals[key].value = value;
        }
      } catch (e) {
        console.error(e);
      }
    },
    async fetchTotalsToday() {
      try {
        let response = await Axios.get('/webapi/v3/energy/total-today');
        this.energyToday.usageTotalHigh = response.data.usageTotalHigh;
        this.energyToday.usageTotalLow = response.data.usageTotalLow;
        this.energyToday.redeliveryTotalHigh = response.data.redeliveryTotalHigh;
        this.energyToday.redeliveryTotalLow = response.data.redeliveryTotalLow;
        this.energyToday.totalSolar = response.data.totalSolar;
        this.energyToday.totalGas = response.data.totalGas;
      } catch (e) {
        console.error(e);
      }
    },
    round(value, digits = 3) {
      let ePlus = `e+${digits}`;
      let eMinus = `e-${digits}`;
      return +(Math.round(value + ePlus) + eMinus);
    },
  },
  computed: {
    nowActive(){
      return this.chartRange === "now";
    },
    hourActive() {
      return this.chartRange === "hour";
    },
    dayActive() {
      return this.chartRange === "day";
    },
    weekActive() {
      return this.chartRange === "week";
    },
    chartConfig() {
      let now = new Date();

      if (this.weekActive)
        now.setHours(now.getHours() - 168);
      if (this.dayActive)
        now.setHours(now.getHours() - 24);
      if (this.hourActive)
        now.setMinutes(now.getMinutes() - 60);
      if (this.nowActive)
        now.setMinutes(now.getMinutes() - 10);
      return {
        params: {
          from: now.toISOString(),
          showRealLast: true
        }
      };
    },
    chartUri() {
      if (this.weekActive)
        return "/webapi/v3/metrics/hours";
      if (this.dayActive)
        return "/webapi/v3/metrics/hours";
      if (this.hourActive)
        return "/webapi/v3/metrics/minutes";
      return "/webapi/v3/metrics/tenseconds";
    }
  }
};
</script>