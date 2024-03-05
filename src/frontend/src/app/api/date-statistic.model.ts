/** An object that holds a request statistic for a date. */
export interface DateStatistic {
  /** Date for which this statistic was generated. */
  date: string;

  /** Total body size of all requests on the date. */
  totalRequestBodySize: number;

  /** Total body size of all responses on the date. */
  totalResponseBodySize: number;

  /** Number of requests on the date. */
  requestCount: number;

  /** Number of aborted requests on the date. */
  abortedRequestCount: number;

  /** Number of failed requests on the date. */
  failedRequestCount: number;

  /** Number of expired requests on the date. */
  expiredRequestCount: number;

  /** Number of errored requests on the date. */
  erroredRequestCount: number;
}
