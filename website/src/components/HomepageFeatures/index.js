import React from 'react';
import clsx from 'clsx';
import styles from './styles.module.css';

const FeatureList = [
  {
    title: 'Peel off the complexity',
    Svg: require('@site/static/img/banana.svg').default,
    description: (
      <>
        Simplify the setup and execution of API mocks, effortlessly
      </>
    ),
  },
  {
    title: 'Pure C# scripting',
    Svg: require('@site/static/img/c-sharp.svg').default,
    description: (
      <>
        You don't need to learn a new specific language or API to add dynamic content to your mocks
      </>
    ),
  },
  {
    title: 'Fake data generation',
    Svg: require('@site/static/img/faker.svg').default,
    description: (
      <>
        Experience the convenience of out-of-the-box built-in fake data generation
      </>
    ),
  },
];

function Feature({Svg, title, description}) {
  return (
    <div className={clsx('col col--4')}>
      <div className="text--center">
        <Svg className={styles.featureSvg} role="img" />
      </div>
      <div className="text--center padding-horiz--md">
        <h3>{title}</h3>
        <p>{description}</p>
      </div>
    </div>
  );
}

export default function HomepageFeatures() {
  return (
    <section className={styles.features}>
      <div className="container">
        <div className="row">
          {FeatureList.map((props, idx) => (
            <Feature key={idx} {...props} />
          ))}
        </div>
      </div>
    </section>
  );
}
