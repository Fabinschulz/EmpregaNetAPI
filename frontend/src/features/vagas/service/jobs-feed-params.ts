export type JobsFeedQueryParams = {
  page: number;
  size: number;
  search?: string;
  city?: string[];
  state?: string[];
  workModel?: string[];
  shift?: string[];
  jobType?: string[];
  experience?: string[];
  area?: string[];
  requirement?: string[];
  benefit?: string[];
  companyId?: number[];
  salaryMin?: number;
  salaryMax?: number;
  pcd?: boolean;
  publishedWithin?: string;
  sort?: string;
};

export const JOBS_FEED_PAGE_SIZE = 20;
