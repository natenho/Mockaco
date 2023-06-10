import React from 'react';
import Layout from '@theme/Layout';
import useDocusaurusContext from '@docusaurus/useDocusaurusContext';
import styles from './videos.module.css';
import ReactPlayer from 'react-player/youtube'

export default function Videos() {
  const context = useDocusaurusContext();
  const siteConfig = context.siteConfig;
  return (
    <Layout
      title={`${siteConfig.title} Videos`}
      description="A collection of Mockaco videos">
      <header>
        <div className={styles.container}>
          <div className="row">
            <div>
              <h1 className="hero__title">Mockaco Videos</h1>
              <p className="hero__subtitle">Videos showing some Mockaco features</p>
            </div>
          </div>
        </div>
      </header>
      <main>
        <div className={styles.container}>
            <div className="row">
                <div className={styles.videoContainer}>
                    <ReactPlayer url='https://www.youtube.com/watch?v=QBnXCgZFzM0' controls={true} />
                </div>
            </div>
        </div>
      </main>
    </Layout>
  );
}