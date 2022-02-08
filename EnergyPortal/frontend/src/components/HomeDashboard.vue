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
                        :class="electricityType.styleClass"
                        v-text="energyNameMapping[type]"
                    ></div>
                    <div
                        class="text-h4 text-center text--secondary mt-2 mb-2"
                        v-text="electricityType.value"
                    ></div>
                    <div class="text-subtitle-1 text-center font-weight-light grey--text darken-2">
                        kWh
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
                                :class="{'pr-3': $vuetify.breakpoint.mdAndUp}"
                            >
                                <electricity-chart
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
                <!--        <v-row-->
                <!--            v-for="electricityType in electricityTypes"-->
                <!--            v-text="electricityType.title"-->
                <!--            :key="`chart-${electricityType.title}`"-->
                <!--        >-->
                <!--          <v-col>-->
                <!--            <v-card-->
                <!--                class="text-center p-10"-->
                <!--                v-text="electricityType.title"-->
                <!--            >-->
                <!--            </v-card>-->
                <!--          </v-col>-->
                <!--        </v-row>-->
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
                        :class="{'pl-3': $vuetify.breakpoint.mdAndUp}"
                    >
                        <v-card
                            class="pa-5"
                        >
                            <div
                                class="text-subtitle-2 text-center grey--text"
                                v-text="'Opname vandaag'"
                            ></div>
                            <div
                                class="text-h4 text-center text--secondary mt-2 mb-2"
                                v-text="round(energyToday.usageTotalHigh + energyToday.usageTotalLow)"
                            ></div>
                            <div class="text-subtitle-1 text-center grey--text">
                                kW
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
                        >
                            <div
                                class="text-subtitle-2 text-center grey--text"
                                v-text="'Teruglevering vandaag'"
                            ></div>
                            <div
                                class="text-h4 text-center text--secondary mt-2 mb-2"
                                v-text="round(energyToday.redeliveryTotalHigh + energyToday.redeliveryTotalLow)"
                            ></div>
                            <div class="text-subtitle-1 text-center grey--text">
                                kW
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
                        >
                            <div
                                class="text-subtitle-2 text-center grey--text"
                                v-text="'Opwekking vandaag'"
                            ></div>
                            <div
                                class="text-h4 text-center text--secondary mt-2 mb-2"
                                v-text="energyToday.totalSolar"
                            ></div>
                            <div class="text-subtitle-1 text-center grey--text">
                                kW
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
                        >
                            <div
                                class="text-subtitle-2 text-center grey--text"
                                v-text="'Gasverbruik vandaag'"
                            ></div>
                            <div
                                class="text-h4 text-center text--secondary mt-2 mb-2"
                                v-text="energyToday.totalGas"
                            ></div>
                            <div class="text-subtitle-1 text-center grey--text">
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
                        >
                            <div
                                class="text-subtitle-2 text-center grey--text"
                                v-text="'Meterstanden'"
                            ></div>
                            <v-simple-table>
                                <template v-slot:default>
                                    <tbody>
                                    <tr
                                        v-for="total in totals"
                                        :key="total.title"
                                    >
                                        <td
                                            class="text--secondary font-weight-bold"
                                            v-text="total.title"
                                        ></td>
                                        <td
                                            class="text--secondary"
                                            v-text="total.value"
                                        ></td>
                                    </tr>
                                    </tbody>
                                </template>
                            </v-simple-table>
                        </v-card>
                    </v-col>
                </v-row>
            </v-col>
        </v-row>
    </div>
</template>

<script>
// @ is an alias to /src
import ElectricityChart from "@/components/ElectricityChart";
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
                styleClass: "blue--text",
                value: "N/A"
            },
            solar: {
                styleClass: "orange--text",
                value: "N/A"
            },
            redelivery: {
                styleClass: "green--text",
                value: "N/A"
            },
            intake: {
                styleClass: "red--text",
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
        chartRange: "hour",
        lastRefresh: null,
        timer: null
    }),
    async mounted () {
        this.initCharts();
        await Promise.all([
            this.fetchChartData(),
            this.fetchTotalsToday(),
            this.fetchTotals(),
            // this.fetchUserSettings()
        ]);
        //
        let self = this;
        this.timer = setInterval(function () {
            if (self.chartRange === "now"){
                self.fetchChartData().then();
            }
            self.fetchLast().then();
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
                    chartData: this.buildChartData([], [], chart.backgroundColor, chart.borderColor),
                    backEndName: chart.backEndName
                };
                
                this.chartNames.push(chart.chartName);
                this.energyNameMapping[chart.backEndName] = chart.chartName;
            }
        },
        buildChartData(data, labels, backgroundColor, borderColor) {
            return {
                datasets: [
                    {
                        backgroundColor: backgroundColor,
                        borderColor: borderColor,
                        data: data
                    }
                ],
                labels: labels
            }
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
                    if (chartData.length > 0 && this.lastRefresh === null)
                        this.latestEnergyValues[backEndName].value = chartData[chartData.length -1];
                    this.electricityCharts[chartName].chartData.datasets[0].data = chartData;
                    this.electricityCharts[chartName].chartData.labels = data.timestamps;
                }
                this.lastRefresh = data.queryTimestamp;
                for (let i = 0; i < this.$refs.electricityChart.length; i++){
                    this.$refs.electricityChart[i].refresh();
                }
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
                        from: this.lastRefresh
                    }
                };
                let response = await Axios.get('webapi/v3/metrics/tenseconds', config);
                let data = response.data;
                this.lastRefresh = data.queryTimestamp;
                if (data.timestamps.length > 0) {
                    let index = data.timestamps.length - 1;
                    for (let energyType of Object.keys(this.latestEnergyValues)) {
                        this.latestEnergyValues[energyType].value = data[energyType][index];
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
                let response = await Axios.get('webapi/v3/energy/total-today');
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
                    from: now.toISOString()
                }
            };
        },
        chartUri() {
            if (this.weekActive)
                return "webapi/v3/metrics/hours";
            if (this.dayActive)
                return "webapi/v3/metrics/hours";
            if (this.hourActive)
                return "webapi/v3/metrics/minutes";
            return "webapi/v3/metrics/tenseconds";
        }
    }
};
</script>