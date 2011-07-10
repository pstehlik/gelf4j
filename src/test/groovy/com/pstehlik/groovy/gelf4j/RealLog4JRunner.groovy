package com.pstehlik.groovy.gelf4j

import org.apache.log4j.Category;
import org.apache.log4j.PropertyConfigurator

/**
 * Log4 example that runs some log messages against a configured GL2 host
 *
 * @author Philip Stehlik
 * @since 0.85
 */
class RealLog4JRunner {
    // our log4j category reference
  Category log1
  Category log2
  Category log3

  public static void main(String[] args) {
    new RealLog4JRunner().doStuff()

  }

  public RealLog4JRunner() {
    initializeLogger()
    log1 = Category.getInstance(RealLog4JRunner.class);
    log2 = Category.getInstance(RealLog4JRunner.class.name+"2");
    log3 = Category.getInstance(RealLog4JRunner.class.name+"3");
//    log.info("Logging initialized.");
//    log.info("RealLog4JRunner - leaving the constructor ...");
  }

  private void initializeLogger() {
    Properties logProperties = new Properties();

    try {
      // load our log4j properties / configuration file
      logProperties.load(
        new StringReader('''
#
# our log4j properties / configuration file
#
# GELF appender
log4j.appender.GELF=com.pstehlik.groovy.gelf4j.appender.Gelf4JAppender
log4j.appender.GELF.graylogServerHost=public-graylog2.taulia.com
log4j.appender.GELF.host=Log With Stack
log4j.appender.GELF.facility=1
log4j.appender.GELF.logStackTraceFromMessage=true

log4j.appender.GELF2=com.pstehlik.groovy.gelf4j.appender.Gelf4JAppender
log4j.appender.GELF2.graylogServerHost=public-graylog2.taulia.com
log4j.appender.GELF2.host=Log No Stack
log4j.appender.GELF2.facility=7
log4j.appender.GELF2.logStackTraceFromMessage=false

log4j.appender.GELF3=com.pstehlik.groovy.gelf4j.appender.Gelf4JAppender
log4j.appender.GELF3.graylogServerHost=public-graylog2.taulia.com
log4j.appender.GELF3.host=Another data Generator
log4j.appender.GELF3.facility=13

# use the STDOUT appender. set the level to INFO.
log4j.category.com.pstehlik.groovy.gelf4j.RealLog4JRunner=INFO, GELF
log4j.category.com.pstehlik.groovy.gelf4j.RealLog4JRunner2=DEBUG, GELF2
log4j.category.com.pstehlik.groovy.gelf4j.RealLog4JRunner3=DEBUG, GELF3
''')
      );
      PropertyConfigurator.configure(logProperties);
    }
    catch (IOException e) {
      throw new RuntimeException("Unable to load logging config");
    }
  }

  void doStuff(){
    try{
      throw new Exception("something gets thrown here!!! wait time was [---]")
    } catch (Exception ex){
      log1.error 'error happened with some text', ex
      log1.error ex
      log2.error 'error happened with some text', ex
      log2.error ex
    }

    return;
    final long baseWaitTime = 5L;
    long waitTime = baseWaitTime
    def count = 10
    def rand = new Random()
    count.times{
      waitTime = baseWaitTime * (rand.nextInt(100)+150)
      Thread.sleep(waitTime)
      log.info("[${it}] Waited [${waitTime}] msec before next log entry")
      if(it%100 == 0){
        log.warn("OMG ${it} mod 100 is 0")
      }
      if(it%300 == 0){
        log.error("OMG ${it} mod 300 is 0")
      }
      if(it%1000 == 0){
        try{
          throw new Exception("something gets thrown here!!! wait time was [${waitTime}]")
        } catch (Exception ex){
          log.error 'error happened with some text', ex
          log.error ex
        }
        def msg = "OMG ${it} mod 1000 is 0"
        println msg
        log.error(msg)
      }
    }
    // Log4J is now loaded; try it
    log.info("Leaving the main method");
  }

  private Category getLog(){
    return this."log${new Random().nextInt(2)+1}"
  }
}
