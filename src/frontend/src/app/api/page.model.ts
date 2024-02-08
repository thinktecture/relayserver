/** Single page of a paginated result. */
export interface Page<T> {
  /** Results for this page. */
  results: T[];

  /** Total amount of data entries available. */
  totalCount: number;

  /** Starting index of the results within all available entries. */
  offset: number;

  /** Requested maximum size of the results. */
  pageSize: number;
}
